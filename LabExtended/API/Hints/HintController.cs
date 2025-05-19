using LabExtended.API.Hints.Interfaces;
using LabExtended.API.Hints.Elements.Personal;

using LabExtended.API.Enums;

using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Core;
using LabExtended.Core.Pooling.Pools;

using LabExtended.Utilities;
using LabExtended.Utilities.Update;

using NorthwoodLib.Pools;

using System.Text;
using System.Diagnostics;

using Hints;

using Mirror;

using HintMessage = LabExtended.API.Messages.HintMessage;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace LabExtended.API.Hints;

/// <summary>
/// Controls in-game hint overlays.
/// </summary>
public static class HintController
{
    #region Temporary Hint Settings

    /// <summary>
    /// Gets the default alignment of temporary hints.
    /// </summary>
    public const HintAlign TemporaryHintAlign = HintAlign.Center;
    
    /// <summary>
    /// Gets the default vertical offset of temporary hints.
    /// </summary>
    public const float TemporaryHintVerticalOffset = -5f;
    
    /// <summary>
    /// Gets the default pixel spacing of temporary hints.
    /// </summary>
    public const int TemporaryHintPixelSpacing = 3;
    
    /// <summary>
    /// Whether or not to auto-wrap temporary hints.
    /// </summary>
    public const bool TemporaryHintAutoWrap = true;

    #endregion

    /// <summary>
    /// Gets the maximum length of a string in a hint.
    /// </summary>
    public const ushort MaxHintTextLength = 65534;

    private static volatile List<HintParameter> paramBuffer;
    private static volatile List<HintElement> elements;

    private static List<HintElement> removeNextFrame;
    
    private static volatile StringBuilder builder;

    private static NetworkWriter writer;
    private static NetworkWriter emptyWriter;

    private static Stopwatch watch;

    private static ArraySegment<byte> emptyData;

    private static int idClock;
    private static float updateInterval;
    private static long tickNum;

    internal static volatile bool sendNextFrame;

    /// <summary>
    /// Gets a list of all active hint elements (not including personal elements).
    /// </summary>
    public static IReadOnlyList<HintElement> Elements => elements;

    /// <summary>
    /// Gets the count of active hint elements (not including personal elements).
    /// </summary>
    public static int ElementCount => elements.Count;

    /// <summary>
    /// Whether or not to forcibly update overlays on the next frame.
    /// </summary>
    public static bool SendNextFrame
    {
        get => sendNextFrame;
        set => sendNextFrame = value;
    }

    static HintController()
    {
        elements = new List<HintElement>();
        paramBuffer = new List<HintParameter>();
        removeNextFrame = new();
        
        watch = new Stopwatch();
        writer = new NetworkWriter();

        emptyWriter = new NetworkWriter();
        emptyWriter.WriteHintData(0f, string.Empty);

        emptyData = emptyWriter.ToArraySegment();

        idClock = 0;

        builder = StringBuilderPool.Shared.Rent();
    }

    /// <summary>
    /// Pauses hints for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    public static void PauseHints(this ExPlayer player) 
        => player.Hints.IsPaused = true;
    
    /// <summary>
    /// Resumes hints for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    public static void ResumeHints(this ExPlayer player) 
        => player.Hints.IsPaused = false;
    
    /// <summary>
    /// Toggles hints for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <returns>true if the player's hints were paused</returns>
    public static bool ToggleHints(this ExPlayer player) 
        => player.Hints.IsPaused = !player.Hints.IsPaused;

    /// <summary>
    /// Sends a new hint.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="content">Text of the hint.</param>
    /// <param name="duration">Duration of the hint (in seconds).</param>
    /// <param name="isPriority">Whether to show the hint immediately.</param>
    /// <param name="parameters">A list of hint parameters to display.</param>
    public static void ShowHint(this ExPlayer player, string content, ushort duration, bool isPriority = false, HintParameter[]? parameters = null)
    {
        if (!player)
            throw new ArgumentNullException(nameof(player));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentNullException(nameof(content));

        if (player.Hints.Queue.Count < 1)
        {
            player.Hints.Queue.Add(new HintMessage(content, duration, isPriority, parameters));
        }
        else
        {
            if (isPriority)
            {
                var curHint = player.Hints.Queue[0];

                player.Hints.RemoveCurrent();

                player.Hints.Queue.Insert(0, new HintMessage(content, duration, isPriority, parameters));
                player.Hints.Queue.Add(curHint);
            }
            else
            {
                player.Hints.Queue.Add(new HintMessage(content, duration, isPriority, parameters));
            }
        }
    }

    /// <summary>
    /// Removes and disables all hint elements.
    /// </summary>
    /// <param name="clearPersonal">Whether or not to remove all personal elements as well.</param>
    public static void ClearHintElements(bool clearPersonal = false)
    {
        elements.ForEach(x =>
        {
            x.IsActive = false;

            x.Id = 0;

            x._tickNum = 0;
            x._prevCompiled = null;

            x.OnDisabled();
        });

        elements.Clear();

        if (clearPersonal)
        {
            foreach (var player in ExPlayer.Players)
            {
                player.HintElements.ForEach(x =>
                {
                    x.IsActive = false;

                    x.Id = 0;

                    x._tickNum = 0;
                    x._prevCompiled = null;

                    x.OnDisabled();
                });

                player.HintElements.Clear();
            }
        }
    }

    /// <summary>
    /// Removes a personal hint element.
    /// </summary>
    /// <param name="target">The player that owns the element.</param>
    /// <typeparam name="T">Type of the element.</typeparam>
    /// <returns>true if the element was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool RemoveHintElement<T>(this ExPlayer target) where T : PersonalHintElement
    {
        if (!target)
            throw new ArgumentNullException(nameof(target));

        if (!TryGetHintElement<T>(target, out var element))
            return false;

        return RemoveHintElement(target, element);
    }

    /// <summary>
    /// Removes a global hint element.
    /// </summary>
    /// <typeparam name="T">Type of the element.</typeparam>
    /// <returns>true if the element was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool RemoveHintElement<T>() where T : HintElement
    {
        if (!TryGetHintElement<T>(out var element))
            return false;

        return RemoveHintElement(element);
    }

    /// <summary>
    /// Removes a personal hint element.
    /// </summary>
    /// <param name="target">The player that owns the element.</param>
    /// <param name="customId">Custom ID of the element.</param>
    /// <returns>true if the element was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool RemoveHintElement(this ExPlayer target, string customId)
    {
        if (!TryGetHintElement(target, customId, out var element))
            return false;

        return RemoveHintElement(element);
    }

    /// <summary>
    /// Removes a global hint element.
    /// </summary>
    /// <param name="customId">Custom ID of the element.</param>
    /// <returns>true if the element was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool RemoveHintElement(string customId)
    {
        if (!TryGetHintElement(customId, out var element))
            return false;

        return RemoveHintElement(element);
    }

    /// <summary>
    /// Removes a personal hint element.
    /// </summary>
    /// <param name="target">The player that owns the element.</param>
    /// <param name="elementId">Assigned ID of the element.</param>
    /// <returns>true if the element was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool RemoveHintElement(this ExPlayer target, int elementId)
    {
        if (!TryGetHintElement(target, elementId, out var element))
            return false;

        return RemoveHintElement(element);
    }

    /// <summary>
    /// Removes a global hint element.
    /// </summary>
    /// <param name="elementId">Assigned ID of the element.</param>
    /// <returns>true if the element was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool RemoveHintElement(int elementId)
    {
        if (!TryGetHintElement(elementId, out var element))
            return false;

        return RemoveHintElement(element);
    }

    /// <summary>
    /// Removes a personal hint element.
    /// </summary>
    /// <param name="target">The player that owns the element.</param>
    /// <param name="hintElement">The element instance to remove.</param>
    /// <returns>true if the element was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool RemoveHintElement(this ExPlayer target, PersonalHintElement hintElement)
    {
        if (hintElement is null)
            throw new ArgumentNullException(nameof(hintElement));

        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (target.HintElements is null || !target.HintElements.Remove(hintElement))
            return false;

        hintElement.IsActive = false;

        hintElement.Id = 0;

        hintElement._prevCompiled = null;
        hintElement._tickNum = 0;

        hintElement.OnDisabled();
        return true;
    }

    /// <summary>
    /// Removes a global hint element.
    /// </summary>
    /// <param name="element">Element instance to remove.</param>
    /// <returns>true if the element was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool RemoveHintElement(this HintElement element)
    {
        if (element is null)
            throw new ArgumentNullException(nameof(element));

        element.IsActive = false;

        if (!elements.Remove(element))
            return false;

        element.Id = 0;

        element._prevCompiled = null;
        element._tickNum = 0;

        element.OnDisabled();
        return true;
    }

    /// <summary>
    /// Adds a personal hint element.
    /// </summary>
    /// <param name="target">The player that owns the element.</param>
    /// <typeparam name="T">Type of the element.</typeparam>
    /// <returns>true if the element was added</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool AddHintElement<T>(this ExPlayer target) where T : PersonalHintElement
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        var instance = Activator.CreateInstance<T>();

        if (instance is null)
            throw new Exception($"Failed to construct type {typeof(T).FullName}");

        return AddHintElement(target, instance);
    }

    /// <summary>
    /// Adds a global hint element.
    /// </summary>
    /// <typeparam name="T">Type of the element.</typeparam>
    /// <returns>true if the element was added</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool AddHintElement<T>() where T : HintElement
    {
        var instance = Activator.CreateInstance<T>();

        if (instance is null)
            throw new Exception($"Failed to construct type {typeof(T).FullName}");

        return AddHintElement(instance);
    }

    /// <summary>
    /// Adds a personal hint element.
    /// </summary>
    /// <param name="target">The player that owns the element.</param>
    /// <param name="hintElement">Element instance to add.</param>
    /// <returns>true if the element was added</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool AddHintElement(this ExPlayer target, PersonalHintElement hintElement)
    {
        if (hintElement is null)
            throw new ArgumentNullException(nameof(hintElement));

        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (target.HintElements is null)
            return false;

        hintElement.Id = idClock++;
        hintElement.Player = target;

        hintElement._prevCompiled = null;
        hintElement._tickNum = 0;

        target.HintElements.Add(hintElement);

        hintElement.OnEnabled();
        hintElement.IsActive = true;

        return true;
    }

    /// <summary>
    /// Adds a global hint element.
    /// </summary>
    /// <param name="element">Element instance to add.</param>
    /// <returns>true if the element was added</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool AddHintElement(this HintElement element)
    {
        if (element is null)
            throw new ArgumentNullException(nameof(element));

        if (element is PersonalHintElement)
            throw new Exception("This element requires a target player.");

        element.Id = idClock++;

        element._prevCompiled = null;
        element._tickNum = 0;

        elements.Add(element);

        element.OnEnabled();
        element.IsActive = true;
        
        return true;
    }

    /// <summary>
    /// Gets a list of global elements matching a predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>A list of matching elements.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IEnumerable<HintElement> GetHintElements(Predicate<HintElement> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        return elements.Where(x => predicate(x));
    }

    /// <summary>
    /// Gets a list of personal elements matching a predicate.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>A list of matching elements.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IEnumerable<PersonalHintElement> GetHintElements(this ExPlayer target,
        Predicate<PersonalHintElement> predicate)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        return target.HintElements?.Where(x => predicate(x)) ?? Array.Empty<PersonalHintElement>();
    }

    /// <summary>
    /// Gets a list of global elements matching a predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>A list of matching elements.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IEnumerable<T> GetHintElements<T>(Predicate<T>? predicate = null) where T : HintElement
    {
        if (predicate is null)
            return elements.Where<T>();

        return elements.Where<T>(x => predicate(x));
    }

    /// <summary>
    /// Gets a list of personal elements matching a predicate.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>A list of matching elements.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IEnumerable<T> GetHintElements<T>(this ExPlayer target, Predicate<T>? predicate = null)
        where T : PersonalHintElement
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (predicate is null)
            return target.HintElements?.Where<T>() ?? Array.Empty<T>();
        
        return target.HintElements?.Where<T>(x => predicate(x)) ?? Array.Empty<T>();
    }

    /// <summary>
    /// Gets a global hint element.
    /// </summary>
    /// <param name="id">Assigned ID of the element.</param>
    /// <returns>Resolved element instance (or null if not found).</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T GetHintElement<T>(int id) where T : HintElement
        => GetHintElement<T>(x => x.Id == id);

    /// <summary>
    /// Gets a personal hint element.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <param name="id">Assigned ID of the element.</param>
    /// <returns>Resolved element instance (or null if not found).</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T GetHintElement<T>(this ExPlayer target, int id) where T : PersonalHintElement
        => GetHintElement<T>(target, x => x.Id == id);

    /// <summary>
    /// Gets a global hint element.
    /// </summary>
    /// <param name="customId">Custom ID of the element.</param>
    /// <returns>Resolved element instance (or null if not found).</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T GetHintElement<T>(string customId) where T : PersonalHintElement
        => GetHintElement<T>(x => x.CompareId(customId));

    /// <summary>
    /// Gets a personal hint element.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <param name="customId">Custom ID of the element.</param>
    /// <returns>Resolved element instance (or null if not found).</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T GetHintElement<T>(this ExPlayer target, string customId) where T : PersonalHintElement
        => GetHintElement<T>(target, x => x.CompareId(customId));

    /// <summary>
    /// Gets a global hint element matching a predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>Resolved element instance (or null if not found).</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static HintElement GetHintElement(Predicate<HintElement> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        return elements.TryGetFirst(x => predicate(x), out var element) ? element : null;
    }

    /// <summary>
    /// Gets a personal hint element matching a predicate.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>Resolved element instance (or null if not found).</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static PersonalHintElement GetHintElement(this ExPlayer target, Predicate<PersonalHintElement> predicate)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        if (target.HintElements != null)
            return target.HintElements.TryGetFirst(x => predicate(x), out var element) ? element : null;

        return null;
    }

    /// <summary>
    /// Gets a global hint element matching a predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>Resolved element instance (or null if not found).</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T GetHintElement<T>(Predicate<T>? predicate = null) where T : HintElement
    {
        if (predicate != null)
            return elements.TryGetFirst<T>(x => predicate(x), out var element) ? element : null;
        else
            return elements.TryGetFirst<T>(out var element) ? element : null;
    }

    /// <summary>
    /// Gets a personal hint element matching a predicate.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <param name="predicate">The predicate.</param>
    /// <returns>Resolved element instance (or null if not found).</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T GetHintElement<T>(this ExPlayer target, Predicate<T>? predicate = null) where T : PersonalHintElement
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (target.HintElements is null)
            return null;
        
        if (predicate != null)
            return target.HintElements.TryGetFirst<T>(x => predicate(x), out var element) ? element : null;
        else
            return target.HintElements.TryGetFirst<T>(out var element) ? element : null;
    }

    /// <summary>
    /// Attempts to find a global hint element.
    /// </summary>
    /// <param name="element">The resolved element instance.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>true if the element was found</returns>
    public static bool TryGetHintElement<T>(out T element) where T : HintElement
        => elements.TryGetFirst(out element);

    /// <summary>
    /// Attempts to find a personal hint element.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <param name="element">The resolved element instance.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>true if the element was found</returns>
    public static bool TryGetHintElement<T>(this ExPlayer target, out T element) where T : PersonalHintElement
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (target.HintElements is null)
        {
            element = null;
            return false;
        }
        
        return target.HintElements.TryGetFirst(out element);
    }

    /// <summary>
    /// Attempts to find a global hint element.
    /// </summary>
    /// <param name="id">The assigned ID of the element.</param>
    /// <param name="element">The resolved element instance.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>true if the element was found</returns>
    public static bool TryGetHintElement<T>(int id, out T element) where T : HintElement
        => TryGetHintElement(x => x.Id == id, out element);

    /// <summary>
    /// Attempts to find a personal hint element.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <param name="id">The assigned ID of the element.</param>
    /// <param name="element">The resolved element instance.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>true if the element was found</returns>
    public static bool TryGetHintElement<T>(this ExPlayer target, int id, out T element) where T : PersonalHintElement
        => TryGetHintElement(target, x => x.Id == id, out element);

    /// <summary>
    /// Attempts to find a global hint element.
    /// </summary>
    /// <param name="id">The assigned ID of the element.</param>
    /// <param name="element">The resolved element instance.</param>
    /// <returns>true if the element was found</returns>
    public static bool TryGetHintElement(int id, out HintElement element)
        => TryGetHintElement(x => x.Id == id, out element);

    /// <summary>
    /// Attempts to find a personal hint element.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <param name="id">The assigned ID of the element.</param>
    /// <param name="element">The resolved element instance.</param>
    /// <returns>true if the element was found</returns>
    public static bool TryGetHintElement(this ExPlayer target, int id, out PersonalHintElement element)
        => TryGetHintElement(target, x => x.Id == id, out element);

    /// <summary>
    /// Attempts to find a global hint element.
    /// </summary>
    /// <param name="customId">The custom ID of the element.</param>
    /// <param name="element">The resolved element instance.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>true if the element was found</returns>
    public static bool TryGetHintElement<T>(string customId, out T element) where T : HintElement
        => TryGetHintElement(x => x.CompareId(customId), out element);

    /// <summary>
    /// Attempts to find a personal hint element.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <param name="customId">The custom ID of the element.</param>
    /// <param name="element">The resolved element instance.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>true if the element was found</returns>
    public static bool TryGetHintElement<T>(this ExPlayer target, string customId, out T element)
        where T : PersonalHintElement
        => TryGetHintElement(target, x => x.CompareId(customId), out element);

    /// <summary>
    /// Attempts to find a global hint element.
    /// </summary>
    /// <param name="customId">The custom ID of the element.</param>
    /// <param name="element">The resolved element instance.</param>
    /// <returns>true if the element was found</returns>
    public static bool TryGetHintElement(string customId, out HintElement element)
        => TryGetHintElement(x => x.CompareId(customId), out element);

    /// <summary>
    /// Attempts to find a personal hint element.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <param name="customId">The custom ID of the element.</param>
    /// <param name="element">The resolved element instance.</param>
    /// <returns>true if the element was found</returns>
    public static bool TryGetHintElement(this ExPlayer target, string customId, out PersonalHintElement element)
        => TryGetHintElement(target, x => x.CompareId(customId), out element);

    /// <summary>
    /// Attempts to find a global hint element matching a predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <param name="element">The resolved element instance.</param>
    /// <returns>true if the element was found</returns>
    public static bool TryGetHintElement<T>(Predicate<T> predicate, out T element) where T : HintElement
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        return elements.TryGetFirst(x => predicate(x), out element);
    }

    /// <summary>
    /// Attempts to find a personal hint element matching a predicate.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <param name="predicate">The predicate.</param>
    /// <param name="element">The resolved element instance.</param>
    /// <returns>true if the element was found</returns>
    public static bool TryGetHintElement<T>(this ExPlayer target, Predicate<T> predicate, out T element)
        where T : PersonalHintElement
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (target.HintElements is null)
        {
            element = null;
            return false;
        }

        return target.HintElements.TryGetFirst(x => predicate(x), out element);
    }

    /// <summary>
    /// Attempts to find a global hint element matching a predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <param name="element">The resolved element instance.</param>
    /// <returns>true if the element was found</returns>
    public static bool TryGetHintElement(Predicate<HintElement> predicate, out HintElement element)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        return elements.TryGetFirst(x => predicate(x), out element);
    }

    /// <summary>
    /// Attempts to find a personal hint element matching a predicate.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <param name="predicate">The predicate.</param>
    /// <param name="element">The resolved element instance.</param>
    /// <returns>true if the element was found</returns>
    public static bool TryGetHintElement(this ExPlayer target, Predicate<PersonalHintElement> predicate,
        out PersonalHintElement element)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (target.HintElements is null)
        {
            element = null;
            return false;
        }

        return target.HintElements.TryGetFirst(x => predicate(x), out element);
    }

    internal static void Update()
    {
        try
        {
            if (ExPlayer.Count < 1)
                return;

            if (!sendNextFrame && updateInterval > 0 && watch.ElapsedMilliseconds < updateInterval)
                return;

            float lowestRate = ApiLoader.ApiConfig.HintSection.UpdateInterval;

            watch.Restart();

            if (tickNum + 1 >= long.MaxValue)
                tickNum = 0;

            tickNum++;

            if (removeNextFrame.Count > 0)
            {
                removeNextFrame.ForEach(element =>
                {
                    try
                    {
                        if (element.IsActive)
                        {
                            element.IsActive = false;
                            element.OnDisabled();
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    elements.Remove(element);
                });
                
                removeNextFrame.Clear();
            }

            for (var i = 0; i < ExPlayer.Count; i++)
            {
                var player = ExPlayer.Players[i];

                if (player?.Hints is null) 
                    continue;
                
                if (player.HintElements is null)
                    continue;

                if (player.elementsToRemove is null)
                    continue;

                if (player.elementsToRemove.Count > 0)
                {
                    player.elementsToRemove.ForEach(element =>
                    {
                        try
                        {
                            if (element.IsActive)
                            {
                                element.IsActive = false;
                                element.OnDisabled();
                            }
                        }
                        catch
                        {
                            // ignored
                        }

                        player.HintElements.Remove(element);
                    });

                    player.elementsToRemove.Clear();
                }

                if (player.Hints.IsPaused) 
                    continue;

                paramBuffer.Clear();

                builder.Clear();
                builder.Append("~\n<line-height=1285%>\n<line-height=0>\n");

                var anyAppended = false;
                var anyOverride = false;

                var overrideParse = false;

                player.Hints.RefreshRatio();

                if (player.Hints.CurrentMessage is null || player.Hints.UpdateTime())
                    player.Hints.NextMessage();

                if (player.Hints.CurrentMessage != null)
                {
                    player.Hints.ParseTemp();

                    paramBuffer.AddRange(player.Hints.CurrentMessage.Parameters);

                    if (player.Hints.TempData.Count > 0)
                    {
                        HintUtils.AppendMessages(player.Hints.TempData, TemporaryHintAlign, builder,
                            player.Hints.LeftOffset);
                        anyAppended = true;
                    }
                }

                void ProcessElement(HintElement element)
                {
                    if (!element.IsActive)
                        return;

                    if (element.Builder is null)
                    {
                        if (!element.nullBuilderWarning)
                        {
                            ApiLog.Warn("Hint Controller", $"Encountered a null Builder while processing element &3{element.GetType().Name}&r (&r{element.CustomId ?? "(null custom ID)"}&r | &6{element.Id}&r");
                            
                            element.nullBuilderWarning = true;
                        }

                        if (element is PersonalHintElement personalHintElement)
                        {
                            if (!player.elementsToRemove.Contains(personalHintElement))
                            {
                                player.elementsToRemove.Add(personalHintElement);
                            }
                        }
                        else if (!removeNextFrame.Contains(element))
                        {
                            removeNextFrame.Add(element);
                        }

                        return;
                    }

                    element.nullBuilderWarning = false;

                    if (element.ClearParameters)
                        element.Parameters.Clear();

                    if (element._tickNum != tickNum)
                    {
                        if (element.ClearBuilderOnUpdate)
                            element.Builder.Clear();

                        element._tickNum = tickNum;
                        element.OnUpdate();
                    }

                    if (!element.ClearBuilderOnUpdate)
                        element.Builder.Clear();

                    if (!element.OnDraw(player) || element.Builder.Length < 1)
                        return;

                    if ((builder.Length + element.Builder.Length) >= MaxHintTextLength)
                    {
                        ApiLog.Warn("Hint API",
                            $"Could not append text from element &1{element.GetType().Name}&r for player &3{player.Nickname}&r (&6{player.UserId}&r) " +
                            $"due to it exceeding maximum allowed limit (&1{builder.Length + element.Builder.Length}&r / &2{MaxHintTextLength}&r)");

                        return;
                    }

                    var content = element.Builder.ToString();

                    if (element.OverridesOthers)
                    {
                        anyOverride = true;

                        builder.Clear();

                        if (element.ShouldParse)
                            builder.Append("~\n<line-height=1285%>\n<line-height=0>\n");
                        else
                            overrideParse = true;
                    }

                    if (!element.ShouldParse)
                    {
                        builder.Append(content);
                        anyAppended = true;
                    }
                    else
                    {
                        if (!element.ShouldCache || element._prevCompiled is null ||
                            element._prevCompiled != content)
                        {
                            element.Data.ForEach(x => ObjectPool<HintData>.Shared.Return(x));
                            element.Data.Clear();

                            element._prevCompiled = content;

                            content = content
                                .Replace("\r\n", "\n")
                                .Replace("\\n", "\n")
                                .Replace("<br>", "\n")
                                .TrimEnd();

                            HintUtils.TrimStartNewLines(ref content, out var count);

                            var offset = element.GetVerticalOffset(player);

                            if (offset == 0f)
                                offset = -count;

                            HintUtils.GetMessages(content, element.Data, offset, element.ShouldWrap,
                                element.GetPixelSpacing(player));
                        }

                        if (element.Data.Count > 0)
                        {
                            paramBuffer.AddRange(element.Parameters);

                            HintUtils.AppendMessages(element.Data, element.GetAlignment(player), builder,
                                player.Hints.LeftOffset);
                            anyAppended = true;
                        }
                    }

                    if (element is IHintRateModifier hintRateModifier)
                    {
                        var requestedInterval = hintRateModifier.GetDesiredDelay(lowestRate);

                        if (requestedInterval >= 0f && requestedInterval < lowestRate)
                            lowestRate = requestedInterval;
                    }
                }

                foreach (var element in elements)
                {
                    if (anyOverride) 
                        break;
                    
                    ProcessElement(element);
                }

                foreach (var personalElement in player.HintElements)
                {
                    if (anyOverride) 
                        break;
                    
                    ProcessElement(personalElement);
                }

                if (!anyAppended)
                {
                    if (!player.Hints.WasClearedAfterEmpty)
                    {
                        player.Connection.Send(emptyData);
                        player.Hints.WasClearedAfterEmpty = true;
                    }

                    continue;
                }

                if (!overrideParse)
                    builder.Append("<voffset=0><line-height=2100%>\n~");

                var text = builder.ToString();

                if (text.Length >= MaxHintTextLength)
                {
                    if (!player.Hints.WasClearedAfterEmpty)
                    {
                        player.Connection.Send(emptyData);
                        player.Hints.WasClearedAfterEmpty = true;
                    }

                    ApiLog.Warn("Hint API",
                        $"The compiled hint is too big! (&1{text.Length}&r / &2{MaxHintTextLength}&r)");
                    continue;
                }

                writer.Reset();
                writer.WriteHintData(ApiLoader.ApiConfig.HintSection.HintDuration, text, paramBuffer);

                paramBuffer.Clear();

                player.Connection.Send(writer.ToArraySegment());
                player.Hints.WasClearedAfterEmpty = false;
            }

            updateInterval = lowestRate;
        }
        catch (Exception ex)
        {
            ApiLog.Error("Hint Controller", ex);
        }

        sendNextFrame = false;
    }

    [LoaderInitialize(1)]
    private static void OnInit()
        => PlayerUpdateHelper.OnUpdate += Update;
}
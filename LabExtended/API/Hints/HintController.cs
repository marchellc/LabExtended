using LabExtended.API.Hints.Elements.Personal;

using LabExtended.API.Enums;

using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Core;

using LabExtended.Utilities;
using LabExtended.Utilities.Update;

using Hints;

using MEC;

using Mirror;

using UnityEngine;

using HintMessage = LabExtended.API.Messages.HintMessage;

#pragma warning disable CS8604 // Possible null reference argument.
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
 
    /// <summary>
    /// Serialized empty hint message data.
    /// </summary>
    public static ArraySegment<byte> EmptyHintMessage { get; }

    private static int idClock;

    /// <summary>
    /// Exposes the controller's internal properties.
    /// </summary>
    public static HintState State { get; } = new();

    /// <summary>
    /// Gets a list of all active hint elements (not including personal elements).
    /// </summary>
    public static List<HintElement> Elements { get; } = new();

    /// <summary>
    /// Gets the count of active hint elements (not including personal elements).
    /// </summary>
    public static int ElementCount => Elements.Count;

    /// <summary>
    /// Whether or not to forcibly update overlays on the next frame.
    /// </summary>
    public static bool SendNextFrame
    {
        get => State.ForceSend;
        set => State.ForceSend = value;
    }

    static HintController()
    {
        using (var writer = NetworkWriterPool.Get())
        {
            writer.WriteHintData(0f, string.Empty);

            var array = new byte[writer.buffer.Length];
            
            Buffer.BlockCopy(writer.buffer, 0, array, 0, writer.Position);

            EmptyHintMessage = new(array, 0, writer.Position);
        }
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
    /// Shows a base-game hint for a specific player.
    /// </summary>
    /// <param name="hub">The player to show the hint to.</param>
    /// <param name="hint">The hint to show.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ShowHint(this ReferenceHub hub, Hint hint)
    {
        if (!ExPlayer.TryGet(hub, out var player))
            return;
        
        ShowHint(player, hint);
    }

    /// <summary>
    /// Shows a base-game hint for a specific player.
    /// </summary>
    /// <param name="player">The player to show the hint to.</param>
    /// <param name="hint">The hint to show.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ShowHint(this ExPlayer player, Hint hint)
    {
        if (player is null)
            throw new ArgumentNullException(nameof(player));
        
        if (hint is null)
            throw new ArgumentNullException(nameof(hint));
        
        if (hint is TextHint textHint)
        {
            player.ShowHint(textHint.Text, (ushort)Mathf.CeilToInt(textHint.DurationScalar), true, textHint.Parameters);
            return;
        }
        
        // Translation hints are shown locally, so just wait until it's over.
        
        player.PauseHints();
        
        SendNextFrame = true;

        Timing.CallDelayed(hint.DurationScalar + 0.1f, player.ResumeHints);
    }

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
        Elements.ForEach(x =>
        {
            x.IsActive = false;

            x.Id = 0;

            x.tickNum = 0;
            x.prevCompiled = null;

            x.OnDisabled();
        });

        Elements.Clear();

        if (clearPersonal)
        {
            foreach (var player in ExPlayer.Players)
            {
                player.HintElements.ForEach(x =>
                {
                    x.IsActive = false;

                    x.Id = 0;

                    x.tickNum = 0;
                    x.prevCompiled = null;

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

        hintElement.prevCompiled = null;
        hintElement.tickNum = 0;

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

        if (!Elements.Remove(element))
            return false;

        element.Id = 0;

        element.prevCompiled = null;
        element.tickNum = 0;

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

        hintElement.prevCompiled = null;
        hintElement.tickNum = 0;

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

        element.prevCompiled = null;
        element.tickNum = 0;

        Elements.Add(element);

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

        return Elements.Where(x => predicate(x));
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
            return Elements.Where<T>();

        return Elements.Where<T>(x => predicate(x));
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

        return Elements.TryGetFirst(x => predicate(x), out var element) ? element : null;
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
            return Elements.TryGetFirst<T>(x => predicate(x), out var element) ? element : null;
        else
            return Elements.TryGetFirst<T>(out var element) ? element : null;
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
        => Elements.TryGetFirst(out element);

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

        return Elements.TryGetFirst(x => predicate(x), out element);
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

        return Elements.TryGetFirst(x => predicate(x), out element);
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

    private static void Update()
    {
        if (!State.ShouldUpdate)
            return;

        State.ResetFrame();
        
        for (var i = 0; i < ExPlayer.Count; i++)
        {
            var player = ExPlayer.Players[i];

            if (player?.Hints is null)
                continue;

            if (player.HintElements is null)
                continue;

            if (player.IsUnverified)
                continue;

            if (player.Hints.IsPaused)
                continue;

            State.ResetPlayer();
            State.Builder.Append("~\n<line-height=1285%>\n<line-height=0>\n");

            HintProcessor.ProcessPlayer(player);

            if (!State.AnyAppended)
            {
                if (!player.Hints.WasClearedAfterEmpty)
                {
                    player.Connection.Send(EmptyHintMessage);
                    player.Hints.WasClearedAfterEmpty = true;
                }

                continue;
            }

            if (!State.AnyOverrideParse)
                State.Builder.Append("<voffset=0><line-height=2100%>\n~");

            var text = State.Builder.ToString();

            if (text.Length >= MaxHintTextLength)
            {
                if (!player.Hints.WasClearedAfterEmpty)
                {
                    player.Connection.Send(EmptyHintMessage);
                    player.Hints.WasClearedAfterEmpty = true;
                }

                ApiLog.Warn("Hint API",
                    $"The compiled hint is too big! (&1{text.Length}&r / &2{MaxHintTextLength}&r)");
                continue;
            }

            State.Writer.Reset();
            State.Writer.WriteHintData(ApiLoader.ApiConfig.HintSection.HintDuration, text, State.Parameters);

            player.Connection.Send(State.Writer.ToArraySegment());
            player.Hints.WasClearedAfterEmpty = false;
        }
    }

    [LoaderInitialize(1)]
    private static void OnInit()
        => PlayerUpdateHelper.OnUpdate += Update;
}
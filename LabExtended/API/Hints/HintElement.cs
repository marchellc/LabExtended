using System.Text;

using Hints;

using LabExtended.API.Enums;

using NorthwoodLib.Pools;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable MemberCanBeProtected.Global

namespace LabExtended.API.Hints;

/// <summary>
/// A base class for all hint elements.
/// </summary>
public abstract class HintElement
{
    /// <summary>
    /// Gets the default vertical offset.
    /// </summary>
    public const float DefaultVerticalOffset = 0f;
    
    /// <summary>
    /// Gets the default pixel line spacing.
    /// </summary>
    public const int DefaultPixelLineSpacing = 3;

    /// <summary>
    /// Gets the default hint alignment.
    /// </summary>
    public const HintAlign DefaultHintAlign = HintAlign.Center;

    internal long _tickNum = 0;
    internal string? _prevCompiled = null;

    /// <summary>
    /// Gets the element's ID.
    /// </summary>
    public int Id { get; internal set; }

    /// <summary>
    /// Gets the element's custom ID.
    /// </summary>
    public string? CustomId { get; set; }

    /// <summary>
    /// Gets the number of processed ticks.
    /// </summary>
    public long TickNumber => _tickNum;

    /// <summary>
    /// Gets the element's string builder.
    /// </summary>
    public StringBuilder? Builder { get; private set; }
    
    /// <summary>
    /// Gets the element's parsing cache.
    /// </summary>
    public List<HintData>? Data { get; private set; }
    
    /// <summary>
    /// Gets a list of the element's parameters.
    /// </summary>
    public List<HintParameter>? Parameters { get; private set; }

    /// <summary>
    /// Whether or not the element is active.
    /// </summary>
    public bool IsActive { get; internal set; }

    /// <summary>
    /// Whether or not the element's result should be parsed.
    /// </summary>
    public virtual bool ShouldParse { get; } = true;
    
    /// <summary>
    /// Whether or not the element's result should be wrapped.
    /// </summary>
    public virtual bool ShouldWrap { get; } = true;
    
    /// <summary>
    /// Whether or not to use the cache.
    /// </summary>
    public virtual bool ShouldCache { get; } = true;

    /// <summary>
    /// Whether or not to display other elements.
    /// </summary>
    public virtual bool OverridesOthers { get; }
    
    /// <summary>
    /// Whether or not to clear the parameter list for every player.
    /// </summary>
    public virtual bool ClearParameters { get; }

    /// <summary>
    /// Gets called once per tick.
    /// </summary>
    public virtual void OnUpdate() { }

    /// <summary>
    /// Gets called when drawing a player's overlay.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <returns>true if the result should be displayed</returns>
    public virtual bool OnDraw(ExPlayer player) => false;

    /// <summary>
    /// Gets called once the element is added.
    /// </summary>
    public virtual void OnEnabled()
    {
        Parameters ??= ListPool<HintParameter>.Shared.Rent();
        Builder ??= StringBuilderPool.Shared.Rent();
        Data ??= ListPool<HintData>.Shared.Rent();
    }

    /// <summary>
    /// Gets called once the element is removed.
    /// </summary>
    public virtual void OnDisabled()
    {
        if (Parameters != null)
            ListPool<HintParameter>.Shared.Return(Parameters);

        if (Builder != null)
            StringBuilderPool.Shared.Return(Builder);

        if (Data != null)
            ListPool<HintData>.Shared.Return(Data);

        Parameters = null;
        Builder = null;
        Data = null;
    }

    /// <summary>
    /// Gets the element's pixel line spacing for a specific player (defaults to <see cref="DefaultPixelLineSpacing"/>).
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <returns>The pixel line spacing.</returns>
    public virtual int GetPixelSpacing(ExPlayer player) 
        => DefaultPixelLineSpacing;
    
    /// <summary>
    /// Gets the element's vertical offset for a specific player (defaults to <see cref="DefaultVerticalOffset"/>).
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <returns>The vertical offset.</returns>
    public virtual float GetVerticalOffset(ExPlayer player) 
        => DefaultVerticalOffset;

    /// <summary>
    /// Gets the element's hint alignment for a specific player (defaults to <see cref="DefaultHintAlign"/>).
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <returns>The hint alignment.</returns>
    public virtual HintAlign GetAlignment(ExPlayer player)
        => DefaultHintAlign;

    /// <summary>
    /// Adds a parameter to the element.
    /// </summary>
    /// <param name="parameter">The parameter to add.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void AddParameter(HintParameter parameter)
    {
        if (parameter is null)
            throw new ArgumentNullException(nameof(parameter));
        
        Parameters.Add(parameter);
    }

    internal bool CompareId(string customId)
        => !string.IsNullOrWhiteSpace(customId) && !string.IsNullOrWhiteSpace(CustomId) && customId == CustomId;
}
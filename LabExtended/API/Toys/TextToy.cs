using LabExtended.API.Images;
using LabExtended.API.Prefabs;
using LabExtended.API.Interfaces;
using LabExtended.Core;
using LabExtended.Core.Networking;
using LabExtended.Images.Playback;

using Mirror;
using NorthwoodLib.Pools;
using UnityEngine;

#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace LabExtended.API.Toys;

/// <summary>
/// <see cref="AdminToys.TextToy"/> wrapper.
/// </summary>
public class TextToy : AdminToy, IWrapper<AdminToys.TextToy>, IPlaybackDisplay
{
    /// <summary>
    /// Gets the format for a singular string.
    /// </summary>
    public const string SingleStringFormat = "{0}";
    
    /// <summary>
    /// Gets the default size of a text toy display.
    /// </summary>
    public static Vector2 DefaultSize { get; } = AdminToys.TextToy.DefaultDisplaySize;

    /// <summary>
    /// Spawns a new text toy.
    /// </summary>
    /// <param name="position">The spawn position of the toy.</param>
    /// <param name="rotation">The spawn rotation of the toy.</param>
    /// <exception cref="Exception"></exception>
    public TextToy(Vector3? position = null, Quaternion? rotation = null) 
        : base(PrefabList.Text.CreateInstance().GetComponent<AdminToys.TextToy>())
    {
        Base = base.Base as AdminToys.TextToy;

        if (Base == null)
            throw new Exception("Could not spawn TextToy");

        PlaybackDisplay = new(this);
        
        Base.NetworkPosition = position ?? Vector3.zero;
        Base.NetworkRotation = rotation ?? Quaternion.identity;
        
        Base.transform.SetPositionAndRotation(Base.NetworkPosition, Base.NetworkRotation);
        
        NetworkServer.Spawn(Base.gameObject);
    }
    
    internal TextToy(AdminToys.TextToy textToy) : base(textToy)
    {
        Base = textToy;
        PlaybackDisplay = new(this);
    }
    
    /// <inheritdoc cref="IWrapper{T}.Base"/>
    public new AdminToys.TextToy Base { get; }

    /// <inheritdoc cref="IPlaybackDisplay.PlaybackDisplay"/>
    public PlaybackBase PlaybackDisplay { get; private set; }

    /// <summary>
    /// Gets the list of arguments.
    /// </summary>
    public SyncList<string> Arguments => Base.Arguments;

    /// <summary>
    /// Gets or sets the size of the display.
    /// </summary>
    public Vector2 Size
    {
        get => Base._displaySize;
        set => Base.Network_displaySize = value;
    }

    /// <summary>
    /// Gets or sets the text format.
    /// </summary>
    public string Format
    {
        get => Base._textFormat;
        set => Base.Network_textFormat = value;
    }

    /// <summary>
    /// Clears the frame and sets it to a custom string.
    /// </summary>
    /// <param name="value">The string to set.</param>
    /// <param name="setFormat">Whether or not to set a custom text format. The string may not be displayed correctly if this is set to false.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Set(string value, bool setFormat = true)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));

        Arguments.Clear();
        
        if (value.Length > MirrorMethods.MaxStringLength)
        {
            SplitStringNonAlloc(value, Arguments, out var format);
            
            if (setFormat)
                Format = format;
        }
        else
        {
            Format = SingleStringFormat;
            
            if (setFormat)
                Arguments.Add(value);
        }
    }

    /// <summary>
    /// Clears the display.
    /// </summary>
    /// <param name="removeFormat">Whether or not to remove the text format.</param>
    public void Clear(bool removeFormat = false)
    {
        if (removeFormat)
            Format = string.Empty;
        
        Arguments.Clear();
    }

    /// <summary>
    /// Adds the specified value to the argument list.
    /// </summary>
    /// <param name="clearArgs">Whether or not to clear the argument list.</param>
    /// <param name="value">The value to add.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Add(bool clearArgs, object value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        
        if (clearArgs)
            Arguments.Clear();
        
        Arguments.Add(value.ToString());
    }

    /// <summary>
    /// Adds the specified list of values to the argument list.
    /// </summary>
    /// <param name="clearArgs">Whether or not to clear the argument list.</param>
    /// <param name="values">The list of values to add.</param>
    public void AddRange(bool clearArgs, IEnumerable<object> values)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));

        if (clearArgs)
            Arguments.Clear();
        
        if (values is IList<object> list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                Arguments.Add(list[i].ToString());
            }
        }
        else
        {
            foreach (var value in values)
            {
                Arguments.Add(value.ToString());
            }
        }
    }

    /// <summary>
    /// Adds the specified list of values to the argument list.
    /// </summary>
    /// <param name="clearArgs">Whether or not to clear the argument list.</param>
    /// <param name="values">The list of values to add.</param>
    public void AddRange(bool clearArgs, params object[] values)
    {
        if (clearArgs)
            Arguments.Clear();
        
        for (var i = 0; i < values.Length; i++)
            Arguments.Add(values[i].ToString());
    }

    /// <inheritdoc cref="IPlaybackDisplay.SetFrame"/>
    public void SetFrame(ImageFrame? frame)
    {
        if (frame is null)
        {
            Clear(true);
            
            Size = DefaultSize;

            if (Scale != Vector3.one)
                Scale = Vector3.one;
            
            return;
        }

        if (Size != frame.File.toyDisplaySize)
            Size = frame.File.toyDisplaySize;

        if (Scale != frame.File.toyStringImageData.Scale)
            Scale = frame.File.toyStringImageData.Scale;

        if (Format is null || Format != frame.toyFrameFormat)
            Format = frame.toyFrameFormat;

        Arguments.Clear();
        Arguments.AddRange(frame.toyFrameData);
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (PlaybackDisplay != null)
        {
            PlaybackDisplay.Dispose();
            PlaybackDisplay = null;
        }
    }

    /// <summary>
    /// Splits a string into a target string collection.
    /// </summary>
    /// <param name="value">The string to split.</param>
    /// <param name="target">The target collection.</param>
    /// <param name="format">Generated text toy string format.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SplitStringNonAlloc(string value, ICollection<string> target, out string format)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value));

        if (target is null)
            throw new ArgumentNullException(nameof(target));

        var valueBuilder = StringBuilderPool.Shared.Rent();
        
        var formatBuilder = StringBuilderPool.Shared.Rent();
        var formatCount = target.Count;

        void AppendFormat()
        {
            formatBuilder.Append('{');
            formatBuilder.Append(formatCount);
            formatBuilder.Append('}');

            formatCount++;
        }

        for (var i = 0; i < value.Length; i++)
        {
            if (valueBuilder.Length + 1 >= MirrorMethods.MaxStringLength)
            {
                AppendFormat();
                
                target.Add(valueBuilder.ToString());
                
                valueBuilder.Clear();
            }
            
            valueBuilder.Append(value[i]);
        }

        format = StringBuilderPool.Shared.ToStringReturn(formatBuilder);
        
        StringBuilderPool.Shared.Return(valueBuilder);
    }
}
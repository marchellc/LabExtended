using LabExtended.API.Interfaces;

using Mirror;

using UnityEngine;

namespace LabExtended.API.Toys;

/// <summary>
/// <see cref="AdminToys.TextToy"/> wrapper.
/// </summary>
public class TextToy : AdminToy, IWrapper<AdminToys.TextToy>, IDisposable
{
    /// <summary>
    /// Gets the default size of a text toy display.
    /// </summary>
    public static Vector2 DefaultSize { get; } = AdminToys.TextToy.DefaultDisplaySize;
    
    internal TextToy(AdminToys.TextToy textToy) : base(textToy)
    {
        Base = textToy;
    }
    
    /// <inheritdoc cref="IWrapper{T}.Base"/>
    public new AdminToys.TextToy Base { get; }
    
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
    /// <param name="value">The value to add.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Add(object value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        
        Arguments.Add(value.ToString());
    }

    /// <summary>
    /// Adds the specified list of values to the argument list.
    /// </summary>
    /// <param name="values">The list of values to add.</param>
    public void AddRange(IEnumerable<object> values)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));

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
    /// <param name="values">The list of values to add.</param>
    public void AddRange(params object[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            Arguments.Add(values[i].ToString());
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (Base != null)
        {
            if (Arguments != null)
            {
                Arguments.Reset();
                Arguments.OnModified -= Base.RefreshText;
            }
        }
    }
}
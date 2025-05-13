using LabExtended.Core;
using LabExtended.Extensions;
using Mirror;

using UnityEngine;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.API.Toys.Primitives;

/// <summary>
/// A primitive toy used to display static images.
/// </summary>
public class PrimitiveImageToy : IDisposable
{
    private GameObject? parent;
    private PrimitiveToy[][]? pixels;
    
    /// <summary>
    /// Gets the height of the image (in pixels).
    /// </summary>
    public int Height { get; }
    
    /// <summary>
    /// Gets the width of the image (in pixels).
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets or sets the parent transform of the image.
    /// </summary>
    public Transform Parent
    {
        get => parent.transform.parent;
        set => parent.transform.parent = value;
    }

    /// <summary>
    /// Gets or sets the position of the image.
    /// </summary>
    public Vector3 Position
    {
        get => parent.transform.parent?.position ?? parent.transform.position;
        set
        {
            if (parent.transform.parent != null)
            {
                parent.transform.parent.position = value;
                return;
            }
            
            parent.transform.position = value;
        }
    }

    /// <summary>
    /// Gets or sets the rotation of the image.
    /// </summary>
    public Quaternion Rotation
    {
        get => parent.transform.parent?.rotation ?? parent.transform.rotation;
        set
        {
            if (parent.transform.parent != null)
            {
                parent.transform.parent.rotation = value;
                return;
            }
            
            parent.transform.rotation = value;
        }
    }

    /// <summary>
    /// Whether the image is static or not (position updates are disabled if true).
    /// </summary>
    public bool IsStatic
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    pixels[x][y].IsStatic = value;
                }    
            }
        }
    }

    /// <summary>
    /// Gets called once the frame changes.
    /// </summary>
    public event Action? Changed;

    /// <summary>
    /// Gets called once the toy is destroyed.
    /// </summary>
    public event Action? Destroyed;
    
    private PrimitiveImageToy(GameObject parent, PrimitiveToy[][] pixels, int height, int width)
    {
        this.parent = parent;
        this.pixels = pixels;
        
        this.Height = height;
        this.Width = width;
    }

    /// <summary>
    /// Sets the current frame.
    /// </summary>
    /// <param name="frame">The frame pixels.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void SetFrame(Color?[,] frame)
    {
        if (frame is null)
            throw new ArgumentNullException(nameof(frame));

        try
        {
            for (var row = 0; row < Width; row++)
            {
                try
                {
                    for (var col = 0; col < Height; col++)
                    {
                        try
                        {
                            var color = frame[row, col];

                            if (color == null)
                                continue;

                            pixels[row][col].Color = color.Value;
                        }
                        catch (Exception ex)
                        {
                            ApiLog.Error("Primitive Image Toy", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ApiLog.Error("Primitive Image Toy", ex);
                }
            }
            
            Changed?.Invoke();
        }
        catch (Exception ex)
        {
            ApiLog.Error("Primitive Image Toy", ex);
        }
    }
    
    /// <summary>
    /// Sets the image to white.
    /// </summary>
    public void Clear()
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                pixels[x][y].Color = Color.white;
            }    
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (pixels != null)
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    NetworkServer.Destroy(pixels[x][y].GameObject);
                }    
            }

            pixels = null;
        }

        if (parent != null)
        { 
            UnityEngine.Object.Destroy(parent);
            
            parent = null;
        }
        
        Destroyed?.InvokeSafe();
        Destroyed = null;
    }

    /// <summary>
    /// Creates a new image with a specific resolution.
    /// </summary>
    /// <param name="height">The height of the image (in pixels).</param>
    /// <param name="width">The width of the image (in pixels).</param>
    /// <param name="scale">The scale of a singular pixel.</param>
    /// <returns>The spawned image toy instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static PrimitiveImageToy Create(int height = 45, int width = 45, float scale = 1f)
    {
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));
        
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));

        var parent = new GameObject($"PrimitiveImageParent_{DateTime.Now.Ticks}");
        var pixels = new List<List<PrimitiveToy>>();
        
        UnityEngine.Object.DontDestroyOnLoad(parent);
        
        var size = scale * 0.05f;
        var centerDelta = scale * 0.05f * width / 2f;
        
        for (var i = height; i > 0; i--)
        {
            var yAxis = i * 0.05f * scale;
            var list = new List<PrimitiveToy>();
            
            pixels.Add(list);
            
            for (int y = width; y > 0; y--)
            {
                var primitive = new PrimitiveToy(null, null, PrimitiveType.Cube)
                {
                    MovementSmoothing = 0,
                    IsStatic = true
                };
                
                var transform = primitive.Transform;
                    
                transform.localScale = new Vector3(size, size, size);
                transform.localPosition = new Vector3(y * 0.05f * scale - centerDelta, yAxis, 0);
                    
                transform.SetParent(parent.transform);

                list.Add(primitive);
            }
        }

        var pixelsArray = pixels.Select(x => x.ToArray()).ToArray();
        
        pixels.ForEach(x => x.Clear());
        pixels.Clear();
        
        return new PrimitiveImageToy(parent, pixelsArray, height, width);
    }
}
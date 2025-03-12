using LabExtended.Core;

using Mirror;

using UnityEngine;

namespace LabExtended.API.Toys.Primitives;

public class PrimitiveImageToy : IDisposable
{
    private GameObject parent;
    private PrimitiveToy[][] pixels;

    private bool isStatic = true;
    
    private Color? color;
    
    public int Height { get; }
    public int Width { get; }

    public Transform Parent
    {
        get => parent.transform.parent;
        set => parent.transform.parent = value;
    }

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

    public bool IsStatic
    {
        get => isStatic;
        set
        {
            if (value == isStatic)
                return;

            isStatic = value;
            
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    pixels[x][y].IsStatic = value;
                }    
            }
        }
    }
    
    private PrimitiveImageToy(GameObject parent, PrimitiveToy[][] pixels, int height, int width)
    {
        this.parent = parent;
        this.pixels = pixels;
        
        this.Height = height;
        this.Width = width;
    }

    public void SetFrame(Color?[,] frame)
    {
        if (frame is null)
            throw new ArgumentNullException(nameof(frame));

        try
        {
            for (int row = 0; row < Width; row++)
            {
                try
                {
                    for (int col = 0; col < Height; col++)
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
        }
        catch (Exception ex)
        {
            ApiLog.Error("Primitive Image Toy", ex);
        }
    }
    
    public void Clear()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                pixels[x][y].Color = Color.white;
            }    
        }
    }

    public void Dispose()
    {
        if (pixels != null)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
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
    }

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
        
        for (int i = height; i > 0; i--)
        {
            var yAxis = i * 0.05f * scale;
            var list = new List<PrimitiveToy>();
            
            pixels.Add(list);
            
            for (int y = width; y > 0; y--)
            {
                var primitive = new PrimitiveToy(PrimitiveType.Cube)
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
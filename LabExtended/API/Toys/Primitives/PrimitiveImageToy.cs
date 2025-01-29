using LabExtended.Extensions;

using Mirror;

using NorthwoodLib.Pools;

using UnityEngine;

namespace LabExtended.API.Toys.Primitives;

public class PrimitiveImageToy : IDisposable
{
    private GameObject parent;
    private PrimitiveToy[][] pixels;

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
        
        for (int x = 0; x < frame.GetLength(0); x++)
        {
            for (int y = 0; y < frame.GetLength(1); y++)
            {
                var color = frame[x, y];
                
                if (color == null)
                    continue;
                
                pixels[x][y].Color = color.Value;
            }
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
            foreach (var array in pixels)
            {
                foreach (var pixel in array)
                {
                    NetworkServer.Destroy(pixel.GameObject);
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
        var pixels = ListPool<PrimitiveToy[]>.Shared.Rent();
        
        var size = scale * 0.05f;
        var centerDelta = scale * 0.05f * width / 2f;
        
        for (int i = height; i > 0; i--)
        {
            var yAxis = i * 0.05f * scale;
            var list = ListPool<PrimitiveToy>.Shared.Rent();
            
            for (int y = width; y > 0; y--)
            {
                var primitive = PrimitiveToy.Spawn(Vector3.zero, x =>
                {
                    x.MovementSmoothing = 0;
                    x.Type = PrimitiveType.Cube;
                    x.IsStatic = false;

                    var transform = x.Transform;
                    
                    transform.localScale = new Vector3(size, size, size);
                    transform.localPosition = new Vector3(y * 0.05f * scale - centerDelta, yAxis, 0);
                    
                    transform.SetParent(parent.transform);
                });

                list.Add(primitive);
            }
            
            pixels.Add(ListPool<PrimitiveToy>.Shared.ToArrayReturn(list));
        }

        var pixelsArray = ListPool<PrimitiveToy[]>.Shared.ToArrayReturn(pixels);
        return new PrimitiveImageToy(parent, pixelsArray, height, width);
    }
}
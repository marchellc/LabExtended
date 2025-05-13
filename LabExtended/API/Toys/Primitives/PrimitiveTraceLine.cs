using AdminToys;

using Mirror;

using UnityEngine;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.API.Toys.Primitives;

/// <summary>
/// Manages a spawned trace line.
/// </summary>
public class PrimitiveTraceLine : IDisposable
{
    /// <summary>
    /// Gets the toy used to show the start position.
    /// </summary>
    public PrimitiveToy StartToy { get; private set; }
    
    /// <summary>
    /// Gets the toy used to show the end position.
    /// </summary>
    public PrimitiveToy EndToy { get; private set; }
    
    /// <summary>
    /// Gets the toy used to show the line.
    /// </summary>
    public PrimitiveToy LineToy { get; private set; }

    /// <summary>
    /// Gets or sets the line's thickness.
    /// </summary>
    public float LineSize { get; set; }

    /// <summary>
    /// Gets or sets the position of the start toy object.
    /// </summary>
    public Vector3 StartPosition
    {
        get => StartToy?.Position ?? Vector3.zero;
        set
        {
            if (StartToy != null)
                StartToy.Position = value;
        }
    }

    /// <summary>
    /// Gets or sets the scale of the start toy object.
    /// </summary>
    public Vector3 StartScale
    {
        get => StartToy?.Scale ?? Vector3.one;
        set
        {
            if (StartToy != null)
                StartToy.Scale = value;
        }
    }

    /// <summary>
    /// Gets or sets the position of the end toy object.
    /// </summary>
    public Vector3 EndPosition
    {
        get => EndToy?.Position ?? Vector3.zero;
        set
        {
            if (EndToy != null)
                EndToy.Position = value;
        }
    }

    /// <summary>
    /// Gets or sets the scale of the end toy object.
    /// </summary>
    public Vector3 EndScale
    {
        get => EndToy?.Scale ?? Vector3.one;
        set
        {
            if (EndToy != null)
                EndToy.Scale = value;
        }
    }

    /// <summary>
    /// Gets or sets the color of the start toy object.
    /// </summary>
    public Color StartColor
    {
        get => StartToy?.Color ?? Color.white;
        set
        {
            if (StartToy != null)
                StartToy.Color = value;
        }
    }

    /// <summary>
    /// Gets or sets the color of the end toy object.
    /// </summary>
    public Color EndColor
    {
        get => EndToy?.Color ?? Color.white;
        set
        {
            if (EndToy != null)
                EndToy.Color = value;
        }
    }

    /// <summary>
    /// Gets or sets the color of the line toy object.
    /// </summary>
    public Color LineColor
    {
        get => LineToy?.Color ?? Color.white;
        set
        {
            if (LineToy != null)
                LineToy.Color = value;
        }
    }

    /// <summary>
    /// Gets or sets the primitive flags of all line toys.
    /// </summary>
    public PrimitiveFlags Flags
    {
        get
        {
            if (StartToy != null)
                return StartToy.Flags;
            
            if (EndToy != null)
                return EndToy.Flags;
            
            return LineToy.Flags;
        }
        set
        {
            if (StartToy != null)
                StartToy.Flags = value;
            
            if (EndToy != null)
                EndToy.Flags = value;
            
            if (LineToy != null)
                LineToy.Flags = value;
        }
    }

    /// <summary>
    /// Gets or sets the type of the start toy.
    /// </summary>
    public PrimitiveType StartType
    {
        get => StartToy?.Type ?? PrimitiveType.Sphere;
        set
        {
            if (StartToy != null)
                StartToy.Type = value;
        }
    }

    /// <summary>
    /// Gets or sets the type of the end toy.
    /// </summary>
    public PrimitiveType EndType
    {
        get => EndToy?.Type ?? PrimitiveType.Sphere;
        set
        {
            if (EndToy != null)
                EndToy.Type = value;
        }
    }

    /// <summary>
    /// Gets or sets the type of the line toy.
    /// </summary>
    public PrimitiveType LineType
    {
        get => LineToy?.Type ?? PrimitiveType.Sphere;
        set
        {
            if (LineToy != null)
                LineToy.Type = value;
        }
    }

    /// <summary>
    /// Spawns a new trace line.
    /// </summary>
    /// <param name="startPosition">The start object position.</param>
    /// <param name="endPosition">The end object position.</param>
    /// <param name="pointScale">The start and end object scale.</param>
    /// <param name="pointType">The start and end object type.</param>
    /// <param name="lineType">The line object type.</param>
    /// <param name="flags">The primitive flags.</param>
    /// <param name="lineSize">The thickness of the line.</param>
    /// <param name="pointColor">The color of the start and end object.</param>
    /// <param name="lineColor">The color of the line.</param>
    public PrimitiveTraceLine(Vector3 startPosition, Vector3 endPosition, Vector3 pointScale, PrimitiveType pointType, 
        PrimitiveType lineType, PrimitiveFlags flags, float lineSize = 0.01f, Color? pointColor = null, Color? lineColor = null)
    {
        StartToy = new PrimitiveToy(startPosition, null, pointType, flags)
        {
            Scale = pointScale,
            Color = pointColor ?? Color.white
        };

        EndToy = new PrimitiveToy(endPosition, null, pointType, flags)
        {
            Scale = pointScale,
            Color = pointColor ?? Color.white
        };

        LineSize = lineSize;
        
        var scale = new Vector3(LineSize, Vector3.Distance(StartPosition, EndPosition) * (LineType is PrimitiveType.Cube ? 1f : 0.5f), LineSize);
        var position = StartPosition + (EndPosition - StartPosition) * 0.5f;
        var rotation = Quaternion.LookRotation(EndPosition - StartPosition) * Quaternion.Euler(90f, 0f, 0f);
        
        LineToy = new PrimitiveToy(position, rotation, lineType, flags)
        {
            Scale = scale,
            Color = lineColor ?? Color.white
        };
    }

    /// <summary>
    /// Spawns a new trace line.
    /// </summary>
    /// <param name="startToy">The primitive used as the start object.</param>
    /// <param name="endToy">The primitive used as the end object.</param>
    /// <param name="lineType">The primitive type of the line object.</param>
    /// <param name="lineSize">The thickness of the line.</param>
    /// <param name="lineColor">The color of the line.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public PrimitiveTraceLine(PrimitiveToy startToy, PrimitiveToy endToy, PrimitiveType lineType = PrimitiveType.Cube, 
        float lineSize = 0.01f, Color? lineColor = null)
    {
        if (startToy is null)
            throw new ArgumentNullException(nameof(startToy));
        
        if (endToy is null)
            throw new ArgumentNullException(nameof(endToy));
        
        StartToy = startToy;
        EndToy = endToy;
        
        LineSize = lineSize;
        
        var scale = new Vector3(LineSize, Vector3.Distance(StartPosition, EndPosition) * (LineType is PrimitiveType.Cube ? 1f : 0.5f), LineSize);
        var position = StartPosition + (EndPosition - StartPosition) * 0.5f;
        var rotation = Quaternion.LookRotation(EndPosition - StartPosition) * Quaternion.Euler(90f, 0f, 0f);
        
        LineToy = new PrimitiveToy(position, rotation, lineType, startToy.Flags)
        {
            Scale = scale,
            Color = lineColor ?? Color.white
        };
    }
    
    /// <summary>
    /// Updates the line toy position, scale and rotation.
    /// <remarks>The line toy will connect the start and end toys after calling this method.</remarks>
    /// </summary>
    public void UpdateLine()
    {
        LineToy.Position = StartPosition + (EndPosition - StartPosition) * 0.5f;
        LineToy.Rotation = Quaternion.LookRotation(EndPosition - StartPosition) * Quaternion.Euler(90f, 0f, 0f);
        LineToy.Scale = new Vector3(LineSize, Vector3.Distance(StartPosition, EndPosition) * (LineType is PrimitiveType.Cube ? 1f : 0.5f), LineSize);
    }

    /// <summary>
    /// Destroys all line toy components.
    /// </summary>
    public void Dispose()
    {
        if (StartToy?.Base != null)
            NetworkServer.Destroy(StartToy.GameObject);
        
        if (EndToy?.Base != null)
            NetworkServer.Destroy(EndToy.GameObject);
        
        if (LineToy?.Base != null)
            NetworkServer.Destroy(LineToy.GameObject);
        
        StartToy = null;
        EndToy = null;
        LineToy = null;
    }
}
using NorthwoodLib.Pools;

using UnityEngine;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace LabExtended.API.Toys.Text;

/// <summary>
/// A toy that can be used to spawn static images.
/// </summary>
public class TextImageToy : IDisposable
{
    private List<string>? imageList;
    
    /// <summary>
    /// Gets the base text toy.
    /// </summary>
    public TextToy Toy { get; private set; }

    /// <summary>
    /// Gets or sets the display size.
    /// </summary>
    public Vector2 Size
    {
        get => Toy.Size;
        set => Toy.Size = value;
    }
    
    /// <summary>
    /// Creates a new <see cref="TextImageToy"/> wrapper instance.
    /// </summary>
    /// <param name="baseToy"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public TextImageToy(TextToy baseToy)
    {
        if (baseToy is null || baseToy.Base == null)
            throw new ArgumentNullException(nameof(baseToy));

        imageList = ListPool<string>.Shared.Rent();
        
        Toy = baseToy;
        Toy.Clear(true);
    }

    /// <summary>
    /// Sets the frame to the specified image list.
    /// </summary>
    /// <param name="splitImage">The image list.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Set(IEnumerable<string> splitImage)
    {
        if (splitImage is null)
            throw new ArgumentNullException(nameof(splitImage));
        
        imageList.Clear();
        imageList.AddRange(splitImage);
        
        if (imageList.Count > 0)
            InitImage();
    }

    /// <summary>
    /// Sets the frame to the specified image.
    /// </summary>
    /// <param name="image"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Set(string image)
    {
        if (string.IsNullOrWhiteSpace(image))
            throw new ArgumentNullException(nameof(image));

        if (image.Length >= ushort.MaxValue)
            throw new Exception("The image string is too long, split it via TextImageToyUtils.");
        
        imageList.Clear();
        imageList.Add(image);
        
        InitImage();
    }

    /// <summary>
    /// Clears the image.
    /// </summary>
    public void Clear()
    {
        Toy.Clear(true);
        
        imageList.Clear();
    }
    
    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (imageList != null)
            ListPool<string>.Shared.Return(imageList);

        imageList = null;
        Toy = null;
    }

    private void InitImage()
    {
        Toy.Arguments.Clear();
        
        var format = TextImageToyUtils.CreateDisplayFormat(imageList.Count);
        
        if (Toy.Format != format)
            Toy.Format = format;
        
        for (var i = 0; i < imageList.Count; i++)
            Toy.Arguments.Add(imageList[i]);
    }
}
using LabExtended.Core;

using UnityEngine;

using System.Reflection;

using HarmonyLib;

namespace LabExtended.API.Images;

/// <summary>
/// Used to load image files.
/// </summary>
public static class ImageLoader
{
    private static MethodInfo loadImage = AccessTools.Method(
        typeof(ImageConversion), 
            nameof(ImageConversion.LoadImage), 
                [typeof(Texture2D), typeof(byte[]), typeof(bool)]);

    /// <summary>
    /// Attempts to load a texture from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the image file.</param>
    /// <param name="texture">The texture that was loaded.</param>
    /// <returns>true if the texture was loaded</returns>
    public static bool TryLoadTexture2D(string filePath, out Texture2D texture)
    {
        texture = null!;   

        if (!File.Exists(filePath))
            return false;

        var data = File.ReadAllBytes(filePath);

        texture = new Texture2D(2, 2);

        var result = loadImage.Invoke(null, [texture, data, false]);

        if (result is not bool boolResult)
        {
            ApiLog.Warn("LabExtended", $"LoadImage() returned unknown type: {result?.GetType().FullName ?? "(null)"}");
            return false;
        }

        if (boolResult)
            return true;

        texture = null!;
        return false;
    }
}
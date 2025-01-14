using UnityEngine;

namespace LabExtended.Extensions;

public static class ColorExtensions
{
    public static Color ToGlowingColor(this Color color)
    {
        FixPrimitiveColor(ref color);
        return new Color(color.r * 50f, color.g * 50f, color.b * 50f, 0.1f);
    }

    public static Color FixPrimitiveColor(this Color color)
    {
        FixPrimitiveColor(ref color);
        return color;
    }

    public static void FixPrimitiveColor(ref Color color)
    {
        var rValue = color.r;
        var gValue = color.g;
        var bValue = color.b;
        var aValue = color.a;

        var changed = false;

        if (rValue > 1f)
        {
            rValue /= 255f;
            changed = true;
        }

        if (gValue > 1f)
        {
            gValue /= 255f;
            changed = true;
        }

        if (bValue > 1f)
        {
            bValue /= 255f;
            changed = true;
        }

        if (aValue > 1f)
        {
            aValue /= 255f;
            changed = true;
        }

        if (changed)
            color = new Color(rValue, gValue, bValue, aValue);
    }
}
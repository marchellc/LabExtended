using System.Globalization;

namespace LabExtended.Commands.Utilities;

using Parameters.Parsers;

internal static class CommandDelegateParsers
{
    internal static bool TryParseByte(string str, out string error, out byte result)
    {
        result = 0;
        error = null;

        if (!byte.TryParse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
        {
            error = $"Could not parse \"{str}\" into a valid byte.";
            return false;
        }

        return true;
    }
    
    internal static bool TryParseSByte(string str, out string error, out sbyte result)
    {
        result = 0;
        error = null;

        if (!sbyte.TryParse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
        {
            error = $"Could not parse \"{str}\" into a valid short byte.";
            return false;
        }

        return true;
    }
    
    internal static bool TryParseShort(string str, out string error, out short result)
    {
        result = 0;
        error = null;

        if (!short.TryParse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
        {
            error = $"Could not parse \"{str}\" into a signed 16-bit integer.";
            return false;
        }

        return true;
    }
    
    internal static bool TryParseUShort(string str, out string error, out ushort result)
    {
        result = 0;
        error = null;

        if (!ushort.TryParse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
        {
            error = $"Could not parse \"{str}\" into a valid unsigned 16-bit integer.";
            return false;
        }

        return true;
    }
    
    internal static bool TryParseInt(string str, out string error, out int result)
    {
        result = 0;
        error = null;

        if (!int.TryParse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
        {
            error = $"Could not parse \"{str}\" into a valid signed 32-bit integer.";
            return false;
        }

        return true;
    }
    
    internal static bool TryParseUInt(string str, out string error, out uint result)
    {
        result = 0;
        error = null;

        if (!uint.TryParse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
        {
            error = $"Could not parse \"{str}\" into a unsigned 32-bit integer.";
            return false;
        }

        return true;
    }
    
    internal static bool TryParseLong(string str, out string error, out long result)
    {
        result = 0;
        error = null;

        if (!long.TryParse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
        {
            error = $"Could not parse \"{str}\" into a signed 64-bit integer.";
            return false;
        }

        return true;
    }
    
    internal static bool TryParseULong(string str, out string error, out ulong result)
    {
        result = 0;
        error = null;

        if (!ulong.TryParse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result))
        {
            error = $"Could not parse \"{str}\" into a unsigned integer.";
            return false;
        }

        return true;
    }
    
    internal static bool TryParseFloat(string str, out string error, out float result)
    {
        result = 0;
        error = null;

        if (!float.TryParse(str, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out result))
        {
            error = $"Could not parse \"{str}\" into a floating point.";
            return false;
        }

        return true;
    }
    
    internal static bool TryParseDouble(string str, out string error, out double result)
    {
        result = 0;
        error = null;

        if (!double.TryParse(str, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out result))
        {
            error = $"Could not parse \"{str}\" into a double.";
            return false;
        }

        return true;
    }
    
    internal static bool TryParseDecimal(string str, out string error, out decimal result)
    {
        result = 0;
        error = null;

        if (!decimal.TryParse(str, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out result))
        {
            error = $"Could not parse \"{str}\" into a decimal.";
            return false;
        }

        return true;
    }

    internal static bool TryParseBool(string str, out string error, out bool result)
    {
        result = false;
        error = null;

        if (!bool.TryParse(str, out result))
        {
            error = $"Could not parse \"{str}\" into a boolean (true / false).";
            return false;
        }

        return true;
    }

    internal static bool TryParseDate(string str, out string error, out DateTime result)
    {
        result = default(DateTime);
        error = null;

        if (!DateTime.TryParse(str, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal |
                                                                      DateTimeStyles.AllowWhiteSpaces |
                                                                      DateTimeStyles.NoCurrentDateDefault, out result))
        {
            error = $"Could not parse \"{str}\" into a date.";
            return false;
        }

        return true;
    }
}
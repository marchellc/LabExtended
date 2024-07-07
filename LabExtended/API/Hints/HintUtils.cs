using Common.Utilities;

using LabExtended.Core;

using System.Text.RegularExpressions;

namespace LabExtended.API.Hints
{
    public static class HintUtils
    {
        public const int MaxLineLength = 60;

        public static readonly Regex SizeTagRegex = new Regex("(?<=<size=)([^>]*)(?=>)", RegexOptions.Compiled);
        public static readonly Regex NewLineRegex = new Regex("\\n|(<[^>]*>)+|\\s*[^<\\s\\r\\n]+[^\\S\\r\\n]*|\\s*", RegexOptions.Compiled);

        public static bool ManageSize(ref string line, ref int size, out bool isEnded)
        {
            if (TryGetSizeTag(line, ref size, out isEnded))
            {
                if (!isEnded)
                    line += "</size>";

                return true;
            }

            return false;
        }

        public static void TrimStartNewLines(ref string str, out int count)
        {
            var i = 0;

            for (i = 0; i < str.Length && str[i] == '\n'; i++)
                continue;

            count = i;
            str = str.Substring(i);
        }

        public static bool TryGetPixelSize(string tag, ref int size)
        {
            if (tag.EndsWith("%") && float.TryParse(tag.Substring(0, tag.Length - 1), out var result))
            {
                size = (int)(result * 35f / 100f);
                return true;
            }
            else if (tag.EndsWith("em") && float.TryParse(tag.Substring(0, tag.Length - 2), out result))
            {
                size = (int)(result * 35f);
                return true;
            }
            else if (char.IsDigit(tag[tag.GetLastIndex()]) && int.TryParse(tag, out size))
                return true;
            else if (tag.EndsWith("px") && int.TryParse(tag.Substring(0, tag.Length - 2), out size))
                return true;
            else
                return false;
        }

        public static bool TryGetSizeTag(string line, ref int sizeTagValue, out bool sizeTagClosed)
        {
            var matches = SizeTagRegex.Matches(line);

            if (matches.Count - 1 >= 0)
            {
                sizeTagClosed = line.IndexOf("</size>", matches[matches.Count - 1].Index, StringComparison.OrdinalIgnoreCase) != -1;
                return TryGetPixelSize(matches[matches.Count - 1].Value, ref sizeTagValue);
            }

            sizeTagClosed = false;
            return false;
        }

        internal static void GetMessages(float vOffset, string content, SortedSet<HintData> messages)
        {
            var matches = NewLineRegex.Matches(content);
            var line = "";
            var size = 1;
            var num = 0;
            var tagEnded = false;
            var clock = 0;
            var anyAdded = false;

            if (HintModule.ShowDebug)
                ExLoader.Debug("Hint API - GetMessages()", $"vOffset={vOffset}");

            foreach (Match match in matches)
            {
                if (!match.Success)
                    continue;

                var text = match.Value;

                if (HintModule.ShowDebug)
                    ExLoader.Debug("Hint API - GetMessages()", $"Regex Match: {match.Value} ({match.Index})");

                if (text == "\n")
                {
                    var managedSize = ManageSize(ref line, ref size, out tagEnded);

                    if (HintModule.ShowDebug)
                        ExLoader.Debug("Hint API - GetMessages()", $"vOffset={vOffset} size={size}");

                    messages.Add(new HintData(line, size, vOffset, clock++));
                    vOffset -= size;

                    if (HintModule.ShowDebug)
                        ExLoader.Debug("Hint API - GetMessages()", $"[TEXT == NEW LINE] Added data line={line} size={size} vOffset={vOffset} id={clock}");

                    if (!tagEnded && managedSize)
                    {
                        line = $"<size={size}>";
                        num = 0;
                    }
                    else
                    {
                        line = "";
                        num = 0;
                    }

                    continue;
                }
                else if (text.StartsWith("<"))
                {
                    line += text;
                    continue;
                }
                else if (num + text.Length <= MaxLineLength)
                {
                    line += text;
                    num += text.Length;
                    continue;
                }
                else
                {
                    line = line.Trim();

                    var managedSize = ManageSize(ref line, ref size, out tagEnded);

                    if (anyAdded)
                        vOffset -= size;

                    messages.Add(new HintData(line, size, vOffset, clock++));

                    if (!anyAdded)
                    {
                        vOffset -= size;
                        anyAdded = true;
                    }

                    if (HintModule.ShowDebug)
                        ExLoader.Debug("Hint API - GetMessages()", $"[ELSE] Added data line={line} size={size} vOffset={vOffset} id={clock}");

                    while (text.Length > MaxLineLength)
                    {
                        var line2 = text.Substring(0, MaxLineLength);

                        managedSize = ManageSize(ref line2, ref size, out tagEnded);

                        if (anyAdded)
                            vOffset -= size;

                        messages.Add(new HintData(line2, size, vOffset, clock++));

                        if (!anyAdded)
                        {
                            vOffset -= size;
                            anyAdded = true;
                        }

                        if (HintModule.ShowDebug)
                            ExLoader.Debug("Hint API - GetMessages()", $"[WHILE] Added data line={line} size={size} vOffset={vOffset} id={clock}");

                        text = text.Substring(MaxLineLength);
                    }

                    line = text;
                    num = text.Length;

                    if (!tagEnded && managedSize)
                        line = $"<size={size}>{line}";
                }
            }

            if (HintModule.ShowDebug)
                ExLoader.Debug("Hint API - GetMessages()", $"Finish line={line} size={size}");

            if (!string.IsNullOrWhiteSpace(line))
            {
                ManageSize(ref line, ref size, out tagEnded);

                if (anyAdded)
                    vOffset -= size;

                messages.Add(new HintData(line, size, vOffset, clock++));

                if (!anyAdded)
                {
                    vOffset -= size;
                    anyAdded = true;
                }

                if (HintModule.ShowDebug)
                    ExLoader.Debug("Hint API - GetMessages()", $"[LAST IF] Added data line={line} size={size} vOffset={vOffset} id={clock}");
            }
        }
    }
}
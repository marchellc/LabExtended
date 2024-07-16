using Common.Utilities;
using LabExtended.API.Hints;
using LabExtended.Core;
using System.Text.RegularExpressions;

namespace LabExtended.Utilities
{
    public static class HintUtils {
        public static readonly Regex SizeTagRegex = new Regex("(?<=<size=)([^>]*)(?=>)", RegexOptions.Compiled);
        //public static readonly Regex SizeEndRegex = new Regex("(?:<\\/size>)", RegexOptions.Compiled);
        public static readonly Regex NewLineRegex = new Regex("\\n|(<[^>]*>)+|\\s*[^<\\s\\r\\n]+[^\\S\\r\\n]*|\\s*", RegexOptions.Compiled);

        public const int PixelsPerEm = 35;

        public static void ManageSize(ref string line, out int size, out bool isEnded)
        {
            if (TryGetSizeTag(line, out size, out isEnded) && !isEnded)
            {
                line += "</size>";
            }
        }

        public static void TrimStartNewLines(ref string str, out int count)
        {
            for (count = 0; count < str.Length && str[count] == '\n'; count++)
                continue;

            str = str.Substring(count);
        }

        public static bool TryGetPixelSize(string tag, out int size)
        {
            if (tag.EndsWith("%") && float.TryParse(tag.Substring(0, tag.Length - 1), out var result))
            {
                size = (int)(result * PixelsPerEm / 100f);
                return true;
            }
            else if (tag.EndsWith("em") && float.TryParse(tag.Substring(0, tag.Length - 2), out result))
            {
                size = (int)(result * PixelsPerEm);
                return true;
            }
            else if (char.IsDigit(tag[tag.Length - 1]) && int.TryParse(tag, out size))
                return true;
            else if (tag.EndsWith("px") && int.TryParse(tag.Substring(0, tag.Length - 2), out size))
                return true;

            size = PixelsPerEm;
            return false;
        }

        public static bool TryGetSizeTag(string line, out int sizeTagValue, out bool sizeTagClosed)
        {
            var matches = SizeTagRegex.Matches(line);

            sizeTagClosed = true;
            if (matches.Count > 0)
            {
                sizeTagClosed = line.IndexOf("</size>", matches[matches.Count - 1].Index, StringComparison.OrdinalIgnoreCase) != -1;
                return TryGetPixelSize(matches[matches.Count - 1].Value, out sizeTagValue);
            }

            sizeTagValue = PixelsPerEm;
            return false;
        }

        internal static void GetMessages(float vOffset, int charsPerLine, string content, List<HintData> messages)
        {
            var matches = NewLineRegex.Matches(content);
            var line = "";
            var pixelSize = PixelsPerEm;
            var lineLength = 0;
            var tagEnded = true;
            var clock = 0;

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
                    ManageSize(ref line, out pixelSize, out tagEnded);

                    if (HintModule.ShowDebug)
                        ExLoader.Debug("Hint API - GetMessages()", $"vOffset={vOffset} size={pixelSize}");

                    if (!messages.IsEmpty()) {
                        vOffset -= pixelSize / (float)PixelsPerEm;
                    }

                    messages.Add(new HintData(line, pixelSize, vOffset, ++clock));

                    if (HintModule.ShowDebug)
                        ExLoader.Debug("Hint API - GetMessages()", $"[TEXT == NEW LINE] Added data line={line} size={pixelSize} vOffset={vOffset} id={clock}");

                    if (!tagEnded)
                    {
                        line = $"<size={pixelSize}>";
                        lineLength = 0;
                    }
                    else
                    {
                        line = "";
                        lineLength = 0;
                    }

                    continue;
                }
                else if (text.StartsWith("<"))
                {
                    line += text;
                    continue;
                }
                else if (lineLength + text.Length <= charsPerLine && charsPerLine > 0)
                {
                    line += text;
                    lineLength += text.Length;
                    continue;
                }
                else
                {
                    line = line.Trim();

                    ManageSize(ref line, out pixelSize, out tagEnded);

                    if (!messages.IsEmpty())
                        vOffset -= pixelSize / (float)PixelsPerEm;

                    messages.Add(new HintData(line, pixelSize, vOffset, ++clock));

                    if (HintModule.ShowDebug)
                        ExLoader.Debug("Hint API - GetMessages()", $"[ELSE] Added data line={line} size={pixelSize} vOffset={vOffset} id={clock}");

                    if (charsPerLine > 0)
                    {
                        while (text.Length > charsPerLine)
                        {
                            var line2 = text.Substring(0, charsPerLine);

                            ManageSize(ref line2, out pixelSize, out tagEnded);

                            if (!messages.IsEmpty())
                                vOffset -= pixelSize / (float)PixelsPerEm;

                            messages.Add(new HintData(line2, pixelSize, vOffset, ++clock));


                            if (HintModule.ShowDebug)
                                ExLoader.Debug("Hint API - GetMessages()", $"[WHILE] Added data line={line} size={pixelSize} vOffset={vOffset} id={clock}");

                            text = text.Substring(charsPerLine);
                        }

                        line = text;
                        lineLength = text.Length;

                        if (!tagEnded)
                            line = $"<size={pixelSize}>{line}";
                    }
                }
            }

            if (HintModule.ShowDebug)
                ExLoader.Debug("Hint API - GetMessages()", $"Finish line={line} size={pixelSize}");

            if (!string.IsNullOrWhiteSpace(line))
            {
                ManageSize(ref line, out pixelSize, out tagEnded);

                if (!messages.IsEmpty())
                    vOffset -= pixelSize / (float)PixelsPerEm;

                messages.Add(new HintData(line, pixelSize, vOffset, ++clock));

                if (HintModule.ShowDebug)
                    ExLoader.Debug("Hint API - GetMessages()", $"[LAST IF] Added data line={line} size={pixelSize} vOffset={vOffset} id={clock}");
            }
        }
    }
}
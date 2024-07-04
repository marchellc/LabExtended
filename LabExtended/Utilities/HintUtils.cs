using Common.Pooling.Pools;
using Common.Utilities;
using LabExtended.Hints.Elements;
using System.Text.RegularExpressions;

namespace LabExtended.Utilities
{
    public static class HintUtils
    {
        public const int MaxLineLength = 60;

        public static readonly Regex TagRegex = new Regex("(?<=<size=)([^>]*)(?=>)", RegexOptions.Compiled);
        public static readonly Regex Regex = new Regex("\\n|(<[^>]*>)+|\\s*[^<\\s\\r\\n]+[^\\S\\r\\n]*|\\s*", RegexOptions.Compiled);

        public static void ManageSize(ref string line, ref int size, out bool isEnded)
        {
            if (TryGetSizeTag(line, out size, out isEnded) && !isEnded)
                line += "</size>";
        }

        public static void TrimStartNewLines(ref string str, out int count)
        {
            var i = 0;

            for (i = 0; i < str.Length && str[i] == '\n'; i++)
                continue;

            count = i;
            str = str.Substring(i);
        }

        public static bool TryGetPixelSize(string tag, out int size)
        {
            if (tag.EndsWith("%") && float.TryParse(tag.Substring(0, tag.Length - 1), out var result))
            {
                size = (int)(result * 35f / 100f);
                return true;
            }
            else if (tag.EndsWith("en") && float.TryParse(tag.Substring(0, tag.Length - 2), out result))
            {
                size = (int)(result * 35f);
                return true;
            }
            else if (char.IsDigit(tag[tag.GetLastIndex()]) && int.TryParse(tag, out size))
                return true;
            else if (tag.EndsWith("px") && int.TryParse(tag.Substring(0, tag.Length - 2), out size))
                return true;
            else
            {
                size = -1;
                return false;
            }
        }

        public static bool TryGetSizeTag(string line, out int sizeTagValue, out bool sizeTagClosed)
        {
            var matches = TagRegex.Matches(line);

            if (matches.Count - 1 >= 0)
            {
                sizeTagClosed = line.IndexOf("</size>", matches[matches.Count - 1].Index, StringComparison.OrdinalIgnoreCase) != -1;
                return TryGetPixelSize(matches[matches.Count - 1].Value, out sizeTagValue);
            }

            sizeTagValue = -1;
            sizeTagClosed = false;

            return false;
        }

        internal static void GetMessages(string content, List<TemporaryElement.TemporaryMessage> messages)
        {
            var matches = Regex.Matches(content);
            var line = "";
            var size = -1;
            var num = 0;
            var tagEnded = false;
            var builder = StringBuilderPool.Shared.Rent();

            foreach (Match match in matches)
            {
                if (!match.Success)
                    continue;

                var text = match.Value;

                if (text == "\n")
                {
                    ManageSize(ref line, ref size, out tagEnded);
                    messages.Add(new TemporaryElement.TemporaryMessage(size, line));

                    if (!tagEnded)
                    {
                        line = $"<size={size}>";
                        num = 0;
                    }
                    else
                    {
                        line = "";
                        num = 0;
                    }
                }
                else if (text.StartsWith("<"))
                {
                    line += text;
                }
                else if (num + text.Length <= MaxLineLength)
                {
                    line += text;
                    num += text.Length;
                }
                else
                {
                    line = line.Trim();

                    ManageSize(ref line, ref size, out tagEnded);

                    messages.Add(new TemporaryElement.TemporaryMessage(size, line));

                    while (text.Length > MaxLineLength)
                    {
                        var line2 = text.Substring(0, MaxLineLength);
                        ManageSize(ref line2, ref size, out tagEnded);
                        messages.Add(new TemporaryElement.TemporaryMessage(size, line2));
                        text = text.Substring(MaxLineLength);
                    }

                    line = text;
                    num = text.Length;

                    if (!tagEnded)
                        line = $"<size={size}>{line}";
                }
            }

            if (!string.IsNullOrWhiteSpace(line))
            {
                ManageSize(ref line, ref size, out tagEnded);
                messages.Add(new TemporaryElement.TemporaryMessage(size, line));
            }
        }
    }
}
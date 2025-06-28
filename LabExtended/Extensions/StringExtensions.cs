using System.Text;
using NorthwoodLib.Pools;

using System.Text.RegularExpressions;
using UnityEngine;

namespace LabExtended.Extensions
{
    public static class StringExtensions
    {
        public const char LogAnsiColorEscapeChar = (char)27;

        public static UTF8Encoding Utf8 { get; } = new(false, true);
        
        public static readonly Regex NewLineRegex = new Regex("r\n|\r|\n", RegexOptions.Compiled);

        public static readonly Regex PascalCaseRegex = new Regex("([a-z,0-9](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", RegexOptions.Compiled);
        public static readonly Regex CamelCaseRegex = new Regex("([A-Z])([A-Z]+)($|[A-Z])", RegexOptions.Compiled);

        public static readonly IReadOnlyList<string> LogAnsiColors = new List<string>()
        {
            "[30m", // Black - &0
            "[31m", // Red = &1
            "[32m", // Green - &2
            "[33m", // Yellow - &3
            "[34m", // Blue - &4
            "[35m", // Purple - &5
            "[36m", // Cyan - &6
            "[37m", // White - &7

            "[0m", // Reset - &r

            "[1m", // Bold On - &b
            "[22m", // Bold Off - &B

            "[3m", // Italic On - &o
            "[23m", // Italic Off - &O

            "[4m", // Underline On - &n
            "[24m", // Underline Off - &N

            "[9m", // Strikethrough On - &m
            "[29m" // Strikethrough Off - &M
        };

        public static void TrimEnds(this string[] strings, params char[] chars)
        {
            for (int i = 0; i < strings.Length; i++)
                strings[i] = strings[i].TrimEnd(chars);
        }

        public static void TrimStarts(this string[] strings, params char[] chars)
        {
            for (int i = 0; i < strings.Length; i++)
                strings[i] = strings[i].TrimStart(chars);
        }

        public static void TrimStrings(this string[] strings)
        {
            for (int i = 0; i < strings.Length; i++)
                strings[i] = strings[i].Trim();
        }

        public static void TrimStrings(this string[] strings, params char[] chars)
        {
            for (int i = 0; i < strings.Length; i++)
                strings[i] = strings[i].Trim(chars);
        }

        public static string RemoveLogAnsiColors(this string str, bool removeTags = false)
        {
            if (removeTags)
                str = str.RemoveHtmlTags();

            foreach (var color in LogAnsiColors)
                str = str.Replace($"{LogAnsiColorEscapeChar}{color}", "");

            return str;
        }

        public static bool TryPeekIndex(this string str, int index, out char value)
        {
            if (index >= str.Length)
            {
                value = default;
                return false;
            }

            value = str[index];
            return true;
        }

        public static List<string> SplitByLength(this string str, int maxLength)
        {
            var list = new List<string>(Mathf.CeilToInt(str.Length / maxLength));
            
            SplitByLength(str, maxLength, list);
            return list;
        }

        public static void SplitByLength(this string str, int maxLength, ICollection<string> target)
        {
            if (maxLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength));

            if (target is null)
                throw new ArgumentNullException(nameof(target));

            while (str.Length > maxLength)
            {
                var otherStr = str.Substring(0, maxLength);

                str = str.Remove(0, maxLength);
                
                target.Add(otherStr);
            }
            
            target.Add(str);
        }
        
        public static void SplitByLengthUtf8(this string str, int maxLength, ICollection<string> target)
        {
            if (maxLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength));

            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (string.IsNullOrEmpty(str))
                return;

            var utf8 = Encoding.UTF8;
            
            var start = 0;
            var length = str.Length;

            while (start < length)
            {
                var end = start;
                var byteCount = 0;

                while (end < length)
                {
                    var charSize = utf8.GetByteCount(new char[] { str[end] });

                    if (byteCount + charSize > maxLength)
                        break;

                    byteCount += charSize;
                    end++;
                }

                var chunk = str.Substring(start, end - start);
                
                target.Add(chunk);
                
                start = end;
            }
        }

        public static string[] SplitLines(this string line)
            => NewLineRegex.Split(line);

        public static bool HasHtmlTags(this string text, out IList<int> openIndexes, out IList<int> closeIndexes)
        {
            openIndexes = Regex.Matches(text, "<").Cast<Match>().Select(m => m.Index).ToList();
            closeIndexes = Regex.Matches(text, ">").Cast<Match>().Select(m => m.Index).ToList();

            return openIndexes.Any() || closeIndexes.Any();
        }

        public static string RemoveHtmlTags(this string text, IList<int> openTagIndexes = null, IList<int> closeTagIndexes = null)
        {
            openTagIndexes ??= Regex.Matches(text, "<").Cast<Match>().Select(m => m.Index).ToList();
            closeTagIndexes ??= Regex.Matches(text, ">").Cast<Match>().Select(m => m.Index).ToList();

            if (closeTagIndexes.Count > 0)
            {
                var sb = StringBuilderPool.Shared.Rent();
                var previousIndex = 0;

                foreach (int closeTagIndex in closeTagIndexes)
                {
                    var openTagsSubset = openTagIndexes.Where(x => x >= previousIndex && x < closeTagIndex);

                    if (openTagsSubset.Count() > 0 && closeTagIndex - openTagsSubset.Max() > 1)
                        sb.Append(text.Substring(previousIndex, openTagsSubset.Max() - previousIndex));
                    else
                        sb.Append(text.Substring(previousIndex, closeTagIndex - previousIndex + 1));

                    previousIndex = closeTagIndex + 1;
                }

                if (closeTagIndexes.Max() < text.Length)
                    sb.Append(text.Substring(closeTagIndexes.Max() + 1));

                return StringBuilderPool.Shared.ToStringReturn(sb);
            }
            else
            {
                return text;
            }
        }

        public static string Remove(this string value, IEnumerable<char> toRemove)
        {
            foreach (var c in toRemove)
                value = value.Replace($"{c}", "");

            return value;
        }

        public static string Remove(this string value, IEnumerable<string> toRemove)
        {
            foreach (var c in toRemove)
                value = value.Replace(c, "");

            return value;
        }

        public static string Remove(this string value, params char[] toRemove)
        {
            foreach (var c in toRemove)
                value = value.Replace($"{c}", "");

            return value;
        }

        public static string Remove(this string value, params string[] toRemove)
        {
            foreach (var str in toRemove)
                value = value.Replace(str, "");

            return value;
        }

        public static string ReplaceWithMap(this string value, params KeyValuePair<string, string>[] stringMap)
            => value.ReplaceWithMap(stringMap.ToDictionary());

        public static string ReplaceWithMap(this string value, params KeyValuePair<char, string>[] charMap)
            => value.ReplaceWithMap(charMap.ToDictionary());

        public static string ReplaceWithMap(this string value, params KeyValuePair<char, char>[] charMap)
            => value.ReplaceWithMap(charMap.ToDictionary());

        public static string ReplaceWithMap(this string value, IDictionary<char, string> charMap)
        {
            foreach (var pair in charMap)
                value = value.Replace(pair.Key.ToString(), pair.Value);

            return value;
        }

        public static string ReplaceWithMap(this string value, IDictionary<char, char> charMap)
        {
            foreach (var pair in charMap)
                value = value.Replace(pair.Key, pair.Value);

            return value;
        }

        public static string ReplaceWithMap(this string value, IDictionary<string, string> stringMap)
        {
            foreach (var pair in stringMap)
                value = value.Replace(pair.Key, pair.Value);

            return value;
        }

        public static bool IsSimilar(this string source, string target, double minScore = 0.9)
            => source.GetSimilarity(target) >= minScore;

        public static double GetSimilarity(this string source, string target)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
                return 0.0;

            if (source == target)
                return 1.0;

            var stepsToSame = GetLevenshteinDistance(source, target);

            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }

        public static int GetLevenshteinDistance(this string source, string target)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
                return 0;

            if (source == target)
                return source.Length;

            var sourceWordCount = source.Length;
            var targetWordCount = target.Length;

            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            var distance = new int[sourceWordCount + 1, targetWordCount + 1];

            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;
            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    var cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }

        public static string FilterWhiteSpaces(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var builder = StringBuilderPool.Shared.Rent();

            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];

                if (i == 0 || c != ' ' || (c == ' ' && input[i - 1] != ' '))
                    builder.Append(c);
            }

            return StringBuilderPool.Shared.ToStringReturn(builder);
        }

        public static bool TrySplit(this string line, char splitChar, bool removeEmptyOrWhitespace, int? length, out string[] splits)
        {
            splits = line.Split(splitChar).Select(str => str.Trim()).ToArray();

            if (removeEmptyOrWhitespace)
                splits = splits.Where(str => !string.IsNullOrWhiteSpace(str)).ToArray();

            if (length.HasValue && splits.Length != length)
                return false;

            return splits.Any();
        }

        public static bool TrySplit(this string line, char[] splitChars, bool removeEmptyOrWhitespace, int? length, out string[] splits)
        {
            splits = line.Split(splitChars).Select(str => str.Trim()).ToArray();

            if (removeEmptyOrWhitespace)
                splits = splits.Where(str => !string.IsNullOrWhiteSpace(str)).ToArray();

            if (length.HasValue && splits.Length != length)
                return false;

            return splits.Any();
        }

        public static string AsString(this IEnumerable<string> values, string separator = "\n")
            => string.Join(separator, values);

        public static string AsString<T>(this IEnumerable<T> values, Func<T, string> convertor, string separator = "\n")
            => string.Join(separator, values.Select(x => convertor(x)));

        public static string AsString<T>(this IEnumerable<T> values, Func<T, string> convertor, Predicate<T> predicate, string separator = "\n")
            => string.Join(separator, values.Where(x => predicate(x)).Select(x => convertor(x)));

        public static string SubstringPostfix(this string str, int index, int length, string postfix = " ...")
            => str.Substring(index, length) + postfix;

        public static string SubstringPostfix(this string str, int length, string postfix = " ...")
            => str.SubstringPostfix(0, length, postfix);

        public static string GetBefore(this string input, char c)
        {
            var start = input.IndexOf(c);

            if (start > 0)
                input = input.Substring(0, start);

            return input;
        }

        public static string GetAfter(this string input, char c)
        {
            var start = input.IndexOf(c);

            if (start > 0)
                input = input.Substring(start, input.Length - start);

            return input;
        }
        
        public static string RemoveBracketsOnEndOfName(this string name)
        {
            int bracketStart = name.IndexOf('(') - 1;

            if (bracketStart > 0)
                name = name.Remove(bracketStart, name.Length - bracketStart);

            return name;
        }

        public static string SnakeCase(this string str)
        {
            if (str is null)
                throw new ArgumentNullException(nameof(str));

            if (str.Length <= 1)
                return str;

            var sb = StringBuilderPool.Shared.Rent();

            sb.Append(char.ToLowerInvariant(str[0]));

            for (int i = 0; i < str.Length; i++)
            {
                if (char.IsUpper(str[i]))
                    sb.Append('_').Append(char.ToLowerInvariant(str[i]));
                else
                    sb.Append(str[i]);
            }

            return StringBuilderPool.Shared.ToStringReturn(sb);
        }

        public static string CamelCase(this string str)
        {
            str = str.Replace("_", "");

            if (str.Length == 0)
                return "null";

            str = CamelCaseRegex.Replace(str, match => match.Groups[1].Value + match.Groups[2].Value.ToLower() + match.Groups[3].Value);

            return char.ToLower(str[0]) + str.Substring(1);
        }

        public static string PascalCase(this string str)
        {
            str = str.CamelCase();
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static string TitleCase(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            var words = str.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length <= 0)
                    continue;

                var c = char.ToUpper(words[i][0]);
                var str2 = "";

                if (words[i].Length > 1)
                    str2 = words[i].Substring(1).ToLower();

                words[i] = c + str2;
            }

            return string.Join(" ", words);
        }

        public static string SpaceByLowerCase(this string str)
        {
            var newStr = "";

            for (int i = 0; i < str.Length; i++)
            {
                if (i == 0)
                {
                    newStr += str[i];
                    continue;
                }

                if ((i + 1) < str.Length && char.IsLower(str[i + 1]))
                {
                    newStr += $" {str[i]}{str[i + 1]}";
                    i += 1;
                    continue;
                }
            }

            return newStr.Trim();
        }

        public static string SpaceByUpperCase(this string str)
            => PascalCaseRegex.Replace(str, "$1 ");

        public static void RemoveTrailingWhiteSpaces(this StringBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            
            while (builder.Length > 0 && char.IsWhiteSpace(builder[builder.Length - 1]))
                builder.Remove(builder.Length - 1, 1);
        }
    }
}

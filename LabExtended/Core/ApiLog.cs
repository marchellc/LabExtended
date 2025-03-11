using System.Diagnostics;

namespace LabExtended.Core
{
    public static class ApiLog
    {
        public static bool IsTrueColorEnabled { get; set; } = true;
        
        public static void Info(object msg) => Info(null, msg);
        public static void Warn(object msg) => Warn(null, msg);
        public static void Error(object msg) => Error(null, msg);
        public static void Debug(object msg) => Debug(null, msg);

        public static void Info(string source, object msg)
        {
	        if (msg is null)
		        throw new ArgumentNullException(nameof(msg));
	        
            if (source is null || source.Length < 1 || source[0] == ' ')
                source = GetSourceType();

            AppendLog($"&7[&b&6INFO&B&7] &7[&b&2{source}&B&7]&r {msg}", ConsoleColor.White);
        }

        public static void Warn(string source, object msg)
        {
	        if (msg is null)
		        throw new ArgumentNullException(nameof(msg));
	        
	        if (source is null || source.Length < 1 || source[0] == ' ')
		        source = GetSourceType();

	        AppendLog($"&7[&b&3WARN&B&7] &7[&b&3{source}&B&7]&r {msg}", ConsoleColor.White);
        }

        public static void Error(string source, object msg)
        {
	        if (msg is null)
		        throw new ArgumentNullException(nameof(msg));
	        
	        if (source is null || source.Length < 1 || source[0] == ' ')
		        source = GetSourceType();

	        AppendLog($"&7[&b&1ERROR&B&7] &7[&b&1{source}&B&7]&r {msg}", ConsoleColor.White);
        }

        public static void Debug(string source, object msg)
        { 
            if (ApiLoader.BaseConfig != null && !ApiLoader.BaseConfig.DebugEnabled)
                return;

            if (source is null || source.Length < 1 || source[0] == ' ')
                source = GetSourceType();

            if (!CheckDebug(source))
                return;

            if (msg is null)
	            throw new ArgumentNullException(nameof(msg));

            AppendLog($"&7[&b&5DEBUG&B&7] &7[&b&5{source}&B&7]&r {msg}", ConsoleColor.White);
        }

        public static bool CheckDebug(string sourceName, bool ifMissingConfig = true)
        {
	        if (ApiLoader.BaseConfig is null)
		        return ifMissingConfig;

            return !string.IsNullOrWhiteSpace(sourceName) && !ApiLoader.BaseConfig.DisabledDebugSources.Contains(sourceName);
        }

        // https://github.com/northwood-studios/NwPluginAPI/blob/master/NwPluginAPI/Core/Log.cs
        // This function was removed in LabAPI so I'm re-adding it.
        
        /// <summary>
        /// Formats color-coded text to ANSI text.
        /// <para>Formatting works as follows:</para>
        /// <para>Each tag MUST start with<b>&</b></para>
        /// <para>Then a singular letter / number that specifies the operation follows.</para>
        /// <para>0 - Black</para>
        /// <para>1 - Red</para>
        /// <para>2 - Green</para>
        /// <para>3 - Yellow</para>
        /// <para>4 - Blue</para>
        /// <para>5 - Purple</para>
        /// <para>6 - Cyan</para>
        /// <para>7 - White</para>
        /// <para>r - Resets all tags</para>
        /// <para>b / B - Bold characters on / off</para>
        /// <para>o / O - Italic characters on / off</para>
        /// <para>m / M - Strikethrough on / off</para>
        /// <para>n / N - Underlinining on / off</para>
        /// </summary>
        /// <param name="message">The message to format.</param>
        /// <param name="defaultColor">The color to use as default.</param>
        /// <param name="unityRichText">Whether or not to convert to Rich Text.</param>
        /// <param name="ignoreTrueColor">Whether or not to ignore true color tags.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatTrueColorText(string message, string defaultColor = "7", bool unityRichText = false, bool ignoreTrueColor = false)
        {
            bool isPrefix = false;
			char escapeChar = (char)27;
			
			string newText = string.Empty;
			string lastTag = string.Empty;

			if (defaultColor != null)
				defaultColor = FormatTrueColorText($"&{defaultColor}", null, unityRichText, ignoreTrueColor);

			for (int x = 0; x < message.Length; x++)
			{
				if (message[x] == '&' && !isPrefix)
				{
					isPrefix = true;
					continue;
				}
				
				if (isPrefix)
				{
					if (!IsTrueColorEnabled && !ignoreTrueColor)
					{
						isPrefix = false;
						continue;
					}

					switch (message[x])
					{
						//Black
						case '0':
							if (unityRichText && lastTag != string.Empty)
								newText += EndTag(ref lastTag);

							newText += unityRichText ? "<color=black>" : $"{escapeChar}[30m";
							lastTag = "color";
							break;
						
						//Red
						case '1':
							if (unityRichText && lastTag != string.Empty)
								newText += EndTag(ref lastTag);

							newText += unityRichText ? "<color=red>" : $"{escapeChar}[31m";
							lastTag = "color";
							break;
						
						//Green
						case '2':
							if (unityRichText && lastTag != string.Empty)
								newText += EndTag(ref lastTag);

							newText += unityRichText ? "<color=green>" : $"{escapeChar}[32m";
							lastTag = "color";
							break;
						
						//Yellow
						case '3':
							if (unityRichText && lastTag != string.Empty)
								newText += EndTag(ref lastTag);

							newText += unityRichText ? "<color=yellow>" : $"{escapeChar}[33m";
							lastTag = "color";
							break;
						
						//Blue
						case '4':
							if (unityRichText && lastTag != string.Empty)
								newText += EndTag(ref lastTag);

							newText += unityRichText ? "<color=blue>" : $"{escapeChar}[34m";
							lastTag = "color";
							break;
						
						//Purple
						case '5':
							if (unityRichText && lastTag != string.Empty)
								newText += EndTag(ref lastTag);

							newText += unityRichText ? "<color=purple>" : $"{escapeChar}[35m";
							lastTag = "color";
							break;
						
						//Cyan
						case '6':
							if (unityRichText && lastTag != string.Empty)
								newText += EndTag(ref lastTag);

							newText += unityRichText ? "<color=cyan>" : $"{escapeChar}[36m";
							lastTag = "color";
							break;
						
						//White
						case '7':
							if (unityRichText && lastTag != string.Empty)
								newText += EndTag(ref lastTag);

							newText += unityRichText ? "<color=white>" : $"{escapeChar}[37m";
							lastTag = "color";
							break;

						//Reset
						case 'r':
							if (unityRichText && lastTag != string.Empty)
							{
								newText += EndTag(ref lastTag) + $"{defaultColor}";
								lastTag = "color";
								break;
							}

							if (!unityRichText)
								newText += $"{escapeChar}[0m";
							break;
						
						//Bold on
						case 'b':
							if (unityRichText && lastTag != string.Empty)
								newText += EndTag(ref lastTag);

							newText += unityRichText ? "<b>" : $"{escapeChar}[1m";
							break;
						
						//Bold off
						case 'B':
							if (unityRichText && lastTag != string.Empty)
								newText += EndTag(ref lastTag);

							newText += unityRichText ? "</b>" : $"{escapeChar}[22m";
							break;
						
						//Italic on
						case 'o':
							if (unityRichText && lastTag != string.Empty)
								newText += EndTag(ref lastTag);

							newText += unityRichText ? "<i>" : $"{escapeChar}[3m";
							break;
						
						//Italic off
						case 'O':
							if (unityRichText && lastTag != string.Empty)
								newText += EndTag(ref lastTag);

							newText += unityRichText ? "</i>" : $"{escapeChar}[23m";
							break;
						
						//Underline on
						case 'n':
							if (unityRichText) break;

							newText += $"{escapeChar}[4m";
							break;
						
						//Underline off
						case 'N':
							if (unityRichText) break;

							newText += $"{escapeChar}[24m";
							break;
						
						//Strikethrough on 
						case 'm':
							if (unityRichText) break;

							newText += $"{escapeChar}[9m";
							break;
						
						//Strikethrough off
						case 'M':
							if (unityRichText) break;

							newText += $"{escapeChar}[29m";
							break;
					}
					
					isPrefix = false;
					continue;
				}
				
				newText += message[x];

				if (unityRichText && x == message.Length - 1 && lastTag != string.Empty)
					newText += EndTag(ref lastTag);
			}

			return newText;
        }
        
        private static string EndTag(ref string currentTag)
        {
	        var saveTag = currentTag;
	        
	        currentTag = string.Empty;
	        return $"</{saveTag}>";
        }

        private static string GetSourceType()
        {
            var trace = new StackTrace();
            var frames = trace.GetFrames();

            foreach (var frame in frames)
            {
                var method = frame.GetMethod();

                if (method is null)
                    continue;

                if (method.DeclaringType is null)
                    continue;

                if (method.DeclaringType == typeof(ApiLog))
                    continue;

                return method.DeclaringType.Name;
            }

            return "Unknown";
        }

        private static void AppendLog(string msg, ConsoleColor color)
        {
	        if (IsTrueColorEnabled)
		        msg = FormatTrueColorText(msg, "7", false, true);

	        ServerConsole.AddLog(msg, color);
        }
    }
}
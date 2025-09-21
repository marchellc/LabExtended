namespace LabExtended.Utilities
{
    /// <summary>
    /// Represents a unique list of items.
    /// </summary>
    public class UniqueList
    {
        /// <summary>
        /// Gets the array of all characters (<see cref="ReadableCharacters"/> + <see cref="UnreadableCharacters"/>).
        /// </summary>
        public static char[] AllCharacters { get; } = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&".ToCharArray();

        /// <summary>
        /// Gets the array of readable characters.
        /// </summary>
        public static char[] ReadableCharacters { get; } = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        /// <summary>
        /// Gets the array of unreadable characters.
        /// </summary>
        public static char[] UnreadableCharacters { get; } = "$%#@!*?;:^&".ToCharArray();

        /// <summary>
        /// A list of already generated values.
        /// </summary>
        public List<object> Generated { get; } = new();

        /// <summary>
        /// Gets a new, unique value.
        /// </summary>
        /// <param name="generator">The delegate used to generate a new value.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public T Get<T>(Func<T> generator)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            var value = generator();

            while (Generated.Contains(value))
                value = generator();

            Generated.Add(value);
            return value;
        }

        /// <summary>
        /// Gets a new, unique string.
        /// </summary>
        /// <param name="maxLength">The maximum length of the string.</param>
        /// <param name="allowUnreadable">Whether or not to allow unreadable characters.</param>
        /// <returns>The generated unique string.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public string GetString(int maxLength, bool allowUnreadable = false)
        {
            if (maxLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength));

            return Get(() =>
            {
                var str = string.Empty;

                while (str.Length != maxLength)
                {
                    str += allowUnreadable
                        ? AllCharacters.RandomItem()
                        : ReadableCharacters.RandomItem();
                }

                return str;
            });
        }
    }
}
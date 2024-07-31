namespace LabExtended.Utilities.Generation
{
    public class UniqueStringGenerator : UniqueGenerator<string>
    {
        public bool AllowUnreadable { get; set; }
        public int StringSize { get; set; }

        public UniqueStringGenerator(int stringSize, bool allowUnreadable)
        {
            if (stringSize < 0)
                throw new ArgumentOutOfRangeException(nameof(stringSize));

            SetGenerator(GenerateString);

            StringSize = stringSize;
            AllowUnreadable = allowUnreadable;
        }

        private string GenerateString()
            => Generator.Instance.GetString(StringSize, AllowUnreadable);
    }
}
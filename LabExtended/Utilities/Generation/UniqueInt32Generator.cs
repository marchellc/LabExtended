namespace LabExtended.Utilities.Generation
{
    public class UniqueInt32Generator : UniqueGenerator<int>
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }

        public UniqueInt32Generator(int minValue = int.MinValue, int maxValue = int.MaxValue)
        {
            SetGenerator(GenerateInt);

            MinValue = minValue;
            MaxValue = maxValue;
        }

        private int GenerateInt()
            => RandomGen.Instance.GetInt32(MinValue, MaxValue);
    }
}
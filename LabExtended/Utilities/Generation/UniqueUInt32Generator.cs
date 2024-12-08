namespace LabExtended.Utilities.Generation
{
    public class UniqueUInt32Generator : UniqueGenerator<uint>
    {
        public uint MinValue { get; set; }
        public uint MaxValue { get; set; }

        public UniqueUInt32Generator(uint minValue = uint.MinValue, uint maxValue = uint.MaxValue)
        {
            SetGenerator(GenerateUInt);

            MinValue = minValue;
            MaxValue = maxValue;
        }

        private uint GenerateUInt()
            => RandomGen.Instance.GetUInt32(MinValue, MaxValue);
    }
}
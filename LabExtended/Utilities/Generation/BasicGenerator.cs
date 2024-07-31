namespace LabExtended.Utilities.Generation
{
    public class BasicGenerator : Generator
    {
        private Random random;

        public BasicGenerator()
            => random = new Random();

        public override int GetInt32(int min = 0, int max = 20)
            => random.Next(min, max);

        public override byte GetByte(byte min = 0, byte max = 255)
            => (byte)random.Next(min, max);

        public override short GetInt16(short min = 0, short max = 20)
            => (short)random.Next(min, max);

        public override sbyte GetSByte(sbyte min = 0, sbyte max = sbyte.MaxValue)
            => (sbyte)random.Next(min, max);

        public override ushort GetUInt16(ushort min = 0, ushort max = 20)
            => (ushort)random.Next(min, max);

        public override uint GetUInt32(uint min = 0, uint max = 20)
        {
            var buffer = new byte[4];

            random.NextBytes(buffer);

            var uint32 = BitConverter.ToUInt32(buffer, 0);

            return (uint)Math.Abs(uint32 % (max - min)) + min;
        }

        public override long GetInt64(long min = 0, long max = 20)
        {
            var buffer = new byte[8];

            random.NextBytes(buffer);

            var int64 = BitConverter.ToInt64(buffer, 0);

            return Math.Abs(int64 % (max - min)) + min;
        }

        public override ulong GetUInt64(ulong min = 0, ulong max = 20)
        {
            var buffer = new byte[8];

            random.NextBytes(buffer);

            var uint64 = BitConverter.ToUInt64(buffer, 0);

            return (ulong)Math.Abs((decimal)uint64 % (max - min)) + min;
        }

        public override float GetFloat(float min = 0, float max = 10)
        {
            var buffer = new byte[4];

            random.NextBytes(buffer);

            var floatVal = BitConverter.ToSingle(buffer, 0);

            return Math.Abs(floatVal % (max - min)) + min;
        }

        public override char GetChar(bool allowUnreadable = false)
        {
            if (allowUnreadable && !GetBool())
                return UnreadableCharacters[random.Next(UnreadableCharacters.Length)];
            else
                return ReadableCharacters[random.Next(ReadableCharacters.Length)];
        }
    }
}
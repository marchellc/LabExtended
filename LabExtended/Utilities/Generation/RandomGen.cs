using LabExtended.API;

namespace LabExtended.Utilities.Generation
{
    public class RandomGen
    {
        public static readonly char[] Characters;
        public static readonly char[] ReadableCharacters;
        public static readonly char[] UnreadableCharacters;

        public static readonly RandomGen Instance;

        static RandomGen()
        {
            Characters = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&".ToCharArray();
            ReadableCharacters = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            UnreadableCharacters = "$%#@!*?;:^&".ToCharArray();

            Instance = new BasicGenerator();
        }

        public virtual char GetChar(bool allowUnreadable = false) { return default; }

        public virtual long GetInt64(long min = 0, long max = 20) { return 0; }
        public virtual ulong GetUInt64(ulong min = 0, ulong max = 20) { return 0; }

        public virtual int GetInt32(int min = 0, int max = 20) { return 0; }
        public virtual uint GetUInt32(uint min = 0, uint max = 20) { return 0; }

        public virtual short GetInt16(short min = 0, short max = 20) { return 0; }
        public virtual ushort GetUInt16(ushort min = 0, ushort max = 20) { return 0; }

        public virtual byte GetByte(byte min = 0, byte max = 255) { return 0; }
        public virtual sbyte GetSByte(sbyte min = 0, sbyte max = 127) { return 0; }

        public virtual float GetFloat(float min = 0f, float max = 10f) { return 0f; }

        public bool GetBool()
            => GetInt32(0, 1) == 1;

        public string GetStringWhitelisted(int minSize = 20, bool allowUnreadable = false, params char[] whitelisted)
        {
            var str = "";

            while (str.Length != minSize && ExServer.IsRunning)
            {
                var c = GetChar(allowUnreadable);

                while (whitelisted.Length > 0 && !whitelisted.Contains(c) && ExServer.IsRunning)
                    c = GetChar(allowUnreadable);

                str += c;
            }

            return str;
        }

        public string GetStringBlacklisted(int minSize = 20, bool allowUnreadable = false, params char[] blacklisted)
        {
            var str = "";

            while (str.Length != minSize && ExServer.IsRunning)
            {
                var c = GetChar(allowUnreadable);

                while (blacklisted.Contains(c) && ExServer.IsRunning)
                    c = GetChar(allowUnreadable);

                str += c;
            }

            return str;
        }

        public string GetString(int minSize = 20, bool allowUnreadable = false)
        {
            var str = "";

            while (str.Length != minSize && ExServer.IsRunning)
                str += GetChar(allowUnreadable);

            return str;
        }
    }
}
using NVorbis;

using Common.Utilities;

namespace LabExtended.API.Audio
{
    public class AudioInfo
    {
        private bool? _isPlaybable = null;

        public string Id { get; }

        public byte[] Data { get; }

        public bool IsPlayable
        {
            get
            {
                if (_isPlaybable.HasValue)
                    return _isPlaybable.Value;

                try
                {
                    using (var stream = new MemoryStream(Data))
                    using (var reader = new VorbisReader(stream))
                    {
                        if (reader.Channels != AudioSettings.Channels)
                            return false;

                        if (reader.SampleRate != AudioSettings.SampleRate)
                            return false;

                        return (_isPlaybable = true).Value;
                    }
                }
                catch { return (_isPlaybable = false).Value; }
            }
        }

        private AudioInfo(string id, byte[] data)
        {
            Id = id;
            Data = data;
        }

        public static AudioInfo GetFile(string filePath, string customId = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (string.IsNullOrWhiteSpace(customId))
                customId = Path.GetFileNameWithoutExtension(filePath);

            return new AudioInfo(customId, File.ReadAllBytes(filePath));
        }

        public static AudioInfo GetRaw(byte[] data, string customId = null)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            if (data.Length < 1)
                throw new ArgumentOutOfRangeException(nameof(data));

            if (string.IsNullOrWhiteSpace(customId))
                customId = Generator.Instance.GetString(10);

            return new AudioInfo(customId, data);
        }
    }
}
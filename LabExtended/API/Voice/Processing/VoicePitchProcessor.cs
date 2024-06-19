namespace LabExtended.API.Voice.Processing
{
    public class VoicePitchProcessor : IVoiceProcessor
    {
        public static float GlobalPitch = 1f;

        public bool IsGloballyActive => GlobalPitch != 1f;

        public bool IsActiveFor(ExPlayer player)
            => (player.VoicePitch != 1f || GlobalPitch != 1f) && player._voicePitch != null;

        public bool SetActiveFor(ExPlayer player)
            => false;

        public bool ProcessData(ExPlayer speaker, ref byte[] data, ref int dataLength)
        {
            if ((speaker.VoicePitch == 1f && GlobalPitch == 1f) || speaker._voicePitch is null)
                return false;

            var pitch = speaker.VoicePitch;

            if (pitch == 1f)
                pitch = GlobalPitch;

            if (pitch == 1f)
                return false;

            var message = new float[48000];

            speaker._voicePitch.Decoder.Decode(data, dataLength, message);
            speaker._voicePitch.PitchShift(pitch, 480U, 48000, message);

            dataLength = speaker._voicePitch.Encoder.Encode(message, data, 480);
            return true;
        }
    }
}
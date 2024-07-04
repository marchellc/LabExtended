using LabExtended.API.Input.Enums;
using LabExtended.API.Input.Interfaces;

namespace LabExtended.API.Input.Inputs
{
    public struct VoiceInputInfo : IInputInfo
    {
        public InputType Type => InputType.NoClip;

        public ExPlayer Player { get; }

        internal VoiceInputInfo(ExPlayer player)
            => Player = player;
    }
}
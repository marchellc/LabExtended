using LabExtended.API.Enums;
using LabExtended.API.Input.Interfaces;

namespace LabExtended.API.Input.Inputs
{
    public struct NoClipInputInfo : IInputInfo
    {
        public InputType Type => InputType.NoClip;

        public ExPlayer Player { get; }

        internal NoClipInputInfo(ExPlayer player)
            => Player = player;
    }
}
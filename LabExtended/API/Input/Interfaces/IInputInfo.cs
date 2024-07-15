using LabExtended.API.Enums;

namespace LabExtended.API.Input.Interfaces
{
    public interface IInputInfo
    {
        ExPlayer Player { get; }

        InputType Type { get; }
    }
}
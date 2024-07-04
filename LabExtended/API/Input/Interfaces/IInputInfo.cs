using LabExtended.API.Input.Enums;

namespace LabExtended.API.Input.Interfaces
{
    public interface IInputInfo
    {
        ExPlayer Player { get; }

        InputType Type { get; }
    }
}
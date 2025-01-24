using Mirror;

namespace LabExtended.API.Settings.Interfaces;

public interface ICustomReaderSetting
{
    void Read(NetworkReader reader);
}
using Mirror;

namespace LabExtended.API.Settings;

public interface ICustomReaderSetting
{
    void Read(NetworkReader reader);
}
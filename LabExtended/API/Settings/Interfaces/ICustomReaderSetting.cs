using Mirror;

namespace LabExtended.API.Settings.Interfaces;

/// <summary>
/// Represents a setting with a custom reader method.
/// </summary>
public interface ICustomReaderSetting
{
    /// <summary>
    /// Reads the network data.
    /// </summary>
    /// <param name="reader">The reader.</param>
    void Read(NetworkReader reader);
}
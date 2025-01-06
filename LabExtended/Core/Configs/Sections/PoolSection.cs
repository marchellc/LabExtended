using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections;

public class PoolSection
{
    [Description("Configures initial pool size per pool type.")]
    public Dictionary<string, int> InitialSizes { get; set; } = new Dictionary<string, int>()
    {
        ["ExamplePool"] = 10
    };
}
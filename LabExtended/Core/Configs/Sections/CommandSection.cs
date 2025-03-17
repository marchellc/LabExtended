using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections;

public class CommandSection
{
    [Description("Whether or not to allow pooling command instances.")]
    public bool AllowInstancePooling { get; set; } = true;
    
    [Description("Whether or not to allow custom command to override vanilla commands.")]
    public bool AllowOverride { get; set; }
}
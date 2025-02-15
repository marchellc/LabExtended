using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections;

public class OptimizationSection
{
    [Description("The minimum required distance between current and previous position.")]
    public float ToyDistanceSync { get; set; } = 0f;
    
    [Description("The minimum required angle between current and previous rotation.")]
    public float ToyAngleSync { get; set; } = 0f;
}
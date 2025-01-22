using LabExtended.API.Enums;

namespace LabExtended.API.Hints.Elements;

public class TestElement : HintElement
{
    public override HintAlign Align { get; } = HintAlign.Center;
    public override bool IsGlobal { get; } = true;

    public override string BuildContent(ExPlayer player)
    {
        var time = DateTime.Now;
        return $"{time.Hour}h {time.Minute}m {time.Second}s {time.Millisecond}ms";
    }
}
using LabExtended.API.Enums;

namespace LabExtended.API.Hints.Elements;

public class PersonalElement : HintElement
{
    public ExPlayer Player { get; internal set; }

    public virtual HintAlign Align { get; } = HintElement.DefaultHintAlign;

    public virtual float VerticalOffset { get; } = HintElement.DefaultVerticalOffset;

    public virtual int PixelSpacing { get; } = HintElement.DefaultPixelLineSpacing;
    
    public virtual bool OnDraw() => false;

    public override HintAlign GetAlignment(ExPlayer player) => Align;
    public override int GetPixelSpacing(ExPlayer player) => PixelSpacing;
    public override float GetVerticalOffset(ExPlayer player) => VerticalOffset;

    public override bool OnDraw(ExPlayer player)
    {
        if (!Player)
            return false;

        return OnDraw();
    }
}
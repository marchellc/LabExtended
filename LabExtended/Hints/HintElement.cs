using LabExtended.API;
using LabExtended.Hints.Enums;
using LabExtended.Hints.Interfaces;

using System.Text;

namespace LabExtended.Hints
{
    public class HintElement : IHintElement
    {
        public bool IsActive { get; set; }
        public float VerticalOffset { get; set; } = 0f;

        public ExPlayer Player { get; set; }
        public HintAlign Alignment { get; set; } = HintAlign.FullLeft;

        public virtual void OnDisabled() { }
        public virtual void OnEnabled() { }

        public virtual void WriteContent(StringBuilder builder) { }
    }
}
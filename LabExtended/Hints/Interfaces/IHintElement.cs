using LabExtended.API;
using LabExtended.Hints.Enums;

using System.Text;

namespace LabExtended.Hints.Interfaces
{
    public interface IHintElement
    {
        bool IsActive { get; set; }

        ExPlayer Player { get; set; }

        HintAlign Alignment { get; set; }

        float VerticalOffset { get; set; }

        void OnEnabled();
        void OnDisabled();

        void WriteContent(StringBuilder builder);
    }
}
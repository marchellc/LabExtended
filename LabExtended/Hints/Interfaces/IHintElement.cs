using LabExtended.API;

using System.Text;

namespace LabExtended.Hints.Interfaces
{
    public interface IHintElement
    {
        bool IsActive { get; set; }

        ExPlayer Player { get; set; }

        HintAlign Alignment { get; set; }

        float VerticalOffset { get; set; }

        StringBuilder Builder { get; }

        void OnEnabled();
        void OnDisabled();

        void Write();
    }
}
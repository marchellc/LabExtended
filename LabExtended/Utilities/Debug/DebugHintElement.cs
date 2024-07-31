using LabExtended.API;
using LabExtended.API.Hints;

namespace LabExtended.Utilities.Debug
{
    public class DebugHintElement : HintElement
    {
        public string ContentToAdd;

        public override void OnEnabled()
        {
            base.OnEnabled();
            ContentToAdd = null;
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            ContentToAdd = null;
        }

        public override string GetContent(ExPlayer _)
            => ContentToAdd;
    }
}
using LabExtended.Hints;

using System.Text;

namespace LabExtended.Utilities
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

        public override void WriteContent(StringBuilder builder)
        {
            base.WriteContent(builder);

            if (!string.IsNullOrWhiteSpace(ContentToAdd))
                builder.Append(ContentToAdd);
        }
    }
}

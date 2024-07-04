using Common.Pooling.Pools;

using LabExtended.API;
using LabExtended.Hints.Interfaces;

using System.Text;

namespace LabExtended.Hints
{
    public class HintElement : IHintElement
    {
        private StringBuilder _builder;

        public bool IsActive { get; set; }
        public ExPlayer Player { get; set; }

        public virtual StringBuilder Builder => _builder;

        public virtual float VerticalOffset { get; set; } = 0f;
        public virtual HintAlign Alignment { get; set; } = HintAlign.FullLeft;

        public virtual void OnDisabled()
        {
            if (_builder is null)
                return;

            StringBuilderPool.Shared.Return(_builder);

            _builder = null;
        }

        public virtual void OnEnabled()
            => _builder ??= StringBuilderPool.Shared.Rent();

        public virtual void Write() { }
    }
}
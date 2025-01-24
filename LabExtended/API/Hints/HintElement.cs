using System.Text;

using LabExtended.API.Enums;
using LabExtended.API.Collections;
using LabExtended.API.Collections.Locked;
using NorthwoodLib.Pools;

namespace LabExtended.API.Hints
{
    public abstract class HintElement
    {
        public const float DefaultVerticalOffset = 0f;
        public const int DefaultPixelLineSpacing = 3;
        
        public const HintAlign DefaultHintAlign = HintAlign.Center;

        internal long _tickNum = 0;
        internal string _prevCompiled = null;

        public int Id { get; internal set; }
        
        public string CustomId { get; set; }

        public long TickNumber => _tickNum;

        public StringBuilder Builder { get; private set; }
        public List<HintData> Data { get; private set; }

        public bool IsActive { get; internal set; }
        
        public virtual bool ShouldParse { get; } = true;
        public virtual bool ShouldWrap { get; } = true;

        public virtual void OnUpdate() { }
        public virtual bool OnDraw(ExPlayer player) => false;

        public virtual void OnEnabled()
        {
            Builder ??= StringBuilderPool.Shared.Rent();
            Data ??= ListPool<HintData>.Shared.Rent();
        }

        public virtual void OnDisabled()
        {
            if (Builder != null)
                StringBuilderPool.Shared.Return(Builder);
            
            if (Data != null)
                ListPool<HintData>.Shared.Return(Data);
            
            Builder = null;
            Data = null;
        }

        public virtual int GetPixelSpacing(ExPlayer player) => DefaultPixelLineSpacing;
        public virtual float GetVerticalOffset(ExPlayer player) => DefaultVerticalOffset;
        
        public virtual HintAlign GetAlignment(ExPlayer player) => DefaultHintAlign;

        internal bool CompareId(string customId)
            => !string.IsNullOrWhiteSpace(customId) && !string.IsNullOrWhiteSpace(CustomId) && customId == CustomId;
    }
}
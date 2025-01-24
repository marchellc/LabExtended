using LabExtended.API.Collections.Locked;
using LabExtended.Core.Pooling;
using LabExtended.Extensions;
using LabExtended.Utilities;

using UnityEngine;

using HintMessage = LabExtended.API.Messages.HintMessage;

namespace LabExtended.API.Hints
{
    public class HintCache : PoolObject
    {
        public ExPlayer Player { get; set; }

        public bool WasClearedAfterEmpty { get; set; }

        public bool IsPaused { get; set; }
        public bool IsParsed { get; set; }

        public float AspectRatio { get; set; } = 0f;
        public float LeftOffset { get; set; } = 0f;

        public List<HintMessage> Queue { get; } = new List<HintMessage>(byte.MaxValue);
        public List<HintData> TempData { get; } = new List<HintData>(byte.MaxValue);

        public HintMessage? CurrentMessage { get; set; }
        
        public float CurrentTime { get; set; }

        public void RemoveCurrent()
        {
            CurrentMessage = null;
            CurrentTime = 0f;

            IsParsed = false;
        }

        public bool RefreshRatio()
        {
            if (AspectRatio != Player.ScreenAspectRatio)
            {
                AspectRatio = Player.ScreenAspectRatio;
                LeftOffset = (int)Math.Round(45.3448f * AspectRatio - 51.527f);

                return true;
            }

            return false;
        }

        public bool UpdateTime()
        {
            if (CurrentMessage is null)
                return false;

            CurrentTime -= Time.deltaTime;

            if (CurrentTime <= 0f)
            {
                CurrentTime = 0f;
                CurrentMessage = null;

                IsParsed = false;
                return true;
            }

            return false;
        }

        public bool NextMessage()
        {
            CurrentMessage = null;
            CurrentTime = 0f;

            if (Queue.Count < 1)
            {
                IsParsed = false;
                return false;
            }

            CurrentMessage = Queue.RemoveAndTake(0);
            CurrentTime = CurrentMessage.Duration;

            IsParsed = false;
            return true;
        }

        public void ParseTemp()
        {
            if (IsParsed)
                return;

            var msg = CurrentMessage.Content;

            TempData.Clear();

            msg = msg.Replace("\r\n", "\n")
                     .Replace("\\n", "\n")
                     .Replace("<br>", "\n")
                     .TrimEnd();

            HintUtils.TrimStartNewLines(ref msg, out _);
            HintUtils.GetMessages(msg, TempData, HintController.TemporaryHintVerticalOffset, HintController.TemporaryHintAutoWrap, HintController.TemporaryHintPixelSpacing);

            IsParsed = true;
        }

        public override void OnReturned()
        {
            base.OnReturned();
            
            Queue.Clear();
            TempData.Clear();

            IsParsed = false;
            IsPaused = false;

            WasClearedAfterEmpty = false;

            AspectRatio = 0f;
            LeftOffset = 0f;
            
            CurrentMessage = null;
            CurrentTime = 0f;

            Player = null;
        }
    }
}

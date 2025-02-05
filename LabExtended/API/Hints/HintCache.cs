using System.Diagnostics;

using LabExtended.Core.Pooling;
using LabExtended.Extensions;
using LabExtended.Utilities;

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

        public Stopwatch Stopwatch { get; } = new Stopwatch();
        
        public HintMessage? CurrentMessage { get; set; }

        public void RemoveCurrent()
        {
            CurrentMessage = null;
            
            IsParsed = false;

            if (Stopwatch.IsRunning)
                Stopwatch.Stop();
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

            if (Stopwatch.ElapsedMilliseconds >= CurrentMessage.Duration)
            {
                RemoveCurrent();
                return true;
            }

            return false;
        }

        public bool NextMessage()
        {
            RemoveCurrent();

            if (Queue.Count < 1)
                return false;

            CurrentMessage = Queue.RemoveAndTake(0);
            
            Stopwatch.Restart();
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
            
            RemoveCurrent();
            
            Queue.Clear();
            TempData.Clear();
            
            IsPaused = false;

            WasClearedAfterEmpty = false;

            AspectRatio = 0f;
            LeftOffset = 0f;

            Player = null;
        }
    }
}

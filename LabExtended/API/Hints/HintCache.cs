using LabExtended.API.Collections.Locked;

using LabExtended.Extensions;
using LabExtended.Utilities;

using UnityEngine;

using HintMessage = LabExtended.API.Messages.HintMessage;

namespace LabExtended.API.Hints
{
    public class HintCache
    {
        public ExPlayer Player;

        public bool WasClearedAfterEmpty = false;

        public bool IsPaused = false;
        public bool IsParsed = false;

        public float AspectRatio = 0f;
        public float LeftOffset= 0f;

        public LockedList<HintMessage> Queue = new LockedList<HintMessage>(byte.MaxValue);
        public LockedHashSet<HintData> TempData = new LockedHashSet<HintData>(byte.MaxValue);

        public HintMessage? CurrentMessage;
        public float CurrentTime;

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
    }
}

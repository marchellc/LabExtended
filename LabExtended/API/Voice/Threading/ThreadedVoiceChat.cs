using LabExtended.Attributes;

using LabExtended.Core;
using LabExtended.Core.Ticking;

using System.Collections.Concurrent;

using VoiceChat.Networking;

namespace LabExtended.API.Voice.Threading
{
    public static class ThreadedVoiceChat
    {
        private static volatile bool m_WasActive;
        private static volatile bool m_RunThread;

        private static volatile Thread m_ProcessingThread;

        private static volatile ConcurrentQueue<ThreadedVoicePacket> m_ProcessingQueue;
        private static volatile ConcurrentQueue<ThreadedVoicePacket> m_OutputQueue;

        public static volatile bool IsPaused;
        public static volatile bool IsEnabled;
        public static volatile bool IsRunning;
        public static volatile bool IsActive;

        public static void Dispose()
        {
            TickDistribution.UnityTick.OnTick -= ProcessQueue;

            m_WasActive = false;
            m_RunThread = false;

            IsPaused = false;
            IsEnabled = false;
            IsActive = false;

            m_ProcessingThread = null;

            IsRunning = false;
        }

        public static void Receive(ExPlayer speaker, ref VoiceMessage msg)
        {
            if (IsPaused || !IsEnabled)
                return;

            var packet = new ThreadedVoicePacket();

            packet.Speaker = speaker;

            packet.Channel = msg.Channel;
            packet.Size = msg.DataLength;

            Copy(ref msg.DataLength, ref msg.Data, ref packet.Data);

            m_ProcessingQueue.Enqueue(packet);
        }

        [LoaderInitialize(1)]
        public static void Start()
        {
            IsEnabled = ApiLoader.ApiConfig.ThreadedVoiceSection.IsEnabled;

            if (!IsEnabled)
            {
                Info("Disabled!", "Loader");
                return;
            }

            TickDistribution.UnityTick.OnTick += ProcessOutput;

            m_WasActive = true;
            m_RunThread = true;

            m_ProcessingQueue = new ConcurrentQueue<ThreadedVoicePacket>();
            m_OutputQueue = new ConcurrentQueue<ThreadedVoicePacket>();

            m_ProcessingThread = new Thread(ProcessQueue);
            m_ProcessingThread.Start();

            Info("Enabled!", "Loader");

            IsRunning = true;
        }

        private static void ProcessInput()
        {
            if (IsPaused || !IsEnabled)
                return;

            while (m_ProcessingQueue.TryDequeue(out var threadedVoice))
            {
                ProcessInput(threadedVoice);
            }
        }

        private static void ProcessInput(ThreadedVoicePacket threadedVoicePacket)
        {
            if (IsPaused || !IsEnabled)
                return;

            foreach (var modifier in VoiceModule.GlobalModifiers)
            {
                if (!modifier.IsEnabled || !modifier.IsThreaded)
                    continue;

                modifier.ModifyThreaded(ref threadedVoicePacket);
            }

            foreach (var modifier in threadedVoicePacket.Speaker.Voice.Modifiers)
            {
                if (!modifier.IsEnabled || !modifier.IsThreaded)
                    continue;

                modifier.ModifyThreaded(ref threadedVoicePacket);
            }

            m_OutputQueue.Enqueue(threadedVoicePacket);
        }

        private static void ProcessOutput()
        {
            if (IsPaused || !IsEnabled)
                return;

            while (m_OutputQueue != null && m_OutputQueue.TryDequeue(out var next))
                next?.Speaker?.Voice?.ReceiveThreaded(next);
        }

        private static void ProcessQueue()
        {
            while (m_RunThread)
            {
                IsActive = !IsPaused && IsEnabled;

                if (!IsEnabled)
                {
                    if (m_WasActive)
                        Dispose();

                    m_WasActive = false;
                    break;
                }

                if (!IsPaused)
                    ProcessInput();
            }
        }

        public static void Copy(ref int length, ref byte[] origData, ref byte[] newData)
        {
            newData = new byte[origData.Length];

            Buffer.BlockCopy(origData, 0, newData, 0, length);
        }

        private static void Info(object msg, string segment = null)
            => ApiLog.Info($"Threaded Voice Chat{(segment != null ? $" / {segment}" : "")}", msg);

        private static void Warn(object msg, string segment = null)
            => ApiLog.Warn($"Threaded Voice Chat{(segment != null ? $" / {segment}" : "")}", msg);

        private static void Error(object msg, string segment = null)
            => ApiLog.Error($"Threaded Voice Chat{(segment != null ? $" / {segment}" : "")}", msg);

        private static void Debug(object msg, string segment = null)
            => ApiLog.Debug($"Threaded Voice Chat{(segment != null ? $" / {segment}" : "")}", msg);
    }
}
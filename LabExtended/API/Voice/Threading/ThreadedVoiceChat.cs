using LabExtended.Attributes;
using LabExtended.Events;

using LabExtended.Core;
using LabExtended.Core.Ticking;
using LabExtended.Core.Profiling;

using System.Collections.Concurrent;

using VoiceChat.Networking;

using Mirror;

using LabExtended.API.Pooling;

namespace LabExtended.API.Voice.Threading
{
    public static class ThreadedVoiceChat
    {
        private static volatile bool m_RunThread;

        private static volatile ProfilerMarker m_ProcessMarker;
        private static volatile ProfilerMarker m_ReceiveMarker;
        private static volatile ProfilerMarker m_OutputMarker;

        private static volatile Thread m_ProcessingThread;

        private static volatile ConcurrentQueue<ThreadedVoicePacket> m_ProcessingQueue;
        private static volatile ConcurrentQueue<ThreadedVoicePacket> m_OutputQueue;

        public static volatile bool IsEnabled;

        public static bool IsRunning => m_RunThread;

        public static void PrintStats()
        {
            m_ProcessMarker.LogStats();
            m_ReceiveMarker.LogStats();
            m_OutputMarker.LogStats();
        }

        public static void Clean()
        {
            m_ProcessMarker.Clear();
            m_ReceiveMarker.Clear();
            m_OutputMarker.Clear();

            m_ProcessingQueue.Clear();
            m_OutputQueue.Clear();
        }

        public static void Dispose()
        {
            RoundEvents.OnWaitingForPlayers -= InternalClean;
            TickManager.OnTick -= ProcessQueue;

            m_RunThread = false;

            m_ProcessMarker?.Dispose();
            m_ProcessMarker = null;

            m_ReceiveMarker?.Dispose();
            m_ReceiveMarker = null;

            m_OutputMarker?.Dispose();
            m_OutputMarker = null;

            m_ProcessingQueue?.Clear();
            m_ProcessingQueue = null;

            m_OutputQueue?.Clear();
            m_OutputQueue = null;

            m_ProcessingThread = null;
        }

        public static void Receive(ExPlayer speaker, ref VoiceMessage msg)
        {
            m_ReceiveMarker.MarkStart();

            var packet = ObjectPool<ThreadedVoicePacket>.Rent();

            packet.Speaker = speaker;

            packet.Channel = msg.Channel;

            packet.Size = msg.DataLength;
            packet.Data = msg.Data;

            m_ProcessingQueue.Enqueue(packet);
            m_ReceiveMarker.MarkEnd();
        }

        [OnLoad]
        public static void Start()
        {
            IsEnabled = ApiLoader.ThreadedVoiceOptions.IsEnabled;

            if (!IsEnabled)
            {
                Info("Disabled!", "Loader");
                return;
            }

            RoundEvents.OnWaitingForPlayers += InternalClean;
            TickManager.OnTick += ProcessOutput;

            m_RunThread = true;

            m_ProcessMarker = new ProfilerMarker("Threaded Voice / Processing Thread", 200);

            m_OutputMarker = new ProfilerMarker("Threaded Voice / Output Handler", 200);
            m_ReceiveMarker = new ProfilerMarker("Threaded Voice / Input Handler", 200);

            m_ProcessingQueue = new ConcurrentQueue<ThreadedVoicePacket>();
            m_OutputQueue = new ConcurrentQueue<ThreadedVoicePacket>();

            m_ProcessingThread = new Thread(ProcessQueue);
            m_ProcessingThread.Start();

            Info("Enabled!", "Loader");
        }

        private static void ProcessInput()
        {
            if (!IsEnabled)
                return;

            while (m_ProcessingQueue.TryDequeue(out var threadedVoice))
                ProcessInput(threadedVoice);
        }

        private static void ProcessInput(ThreadedVoicePacket threadedVoicePacket)
        {
            IsEnabled = !ApiLoader.VoiceOptions.DisableCustomVoice && ApiLoader.ThreadedVoiceOptions.IsEnabled;

            if (!IsEnabled)
                return;

            foreach (var modifier in VoiceModule.GlobalModifiers)
            {
                if (!modifier.IsThreaded || !modifier.IsEnabled)
                    continue;

                modifier.ModifyThreaded(ref threadedVoicePacket);
            }

            foreach (var modifier in threadedVoicePacket.Speaker.Voice.Modifiers)
            {
                if (!modifier.IsThreaded || !modifier.IsEnabled)
                    continue;

                modifier.ModifyThreaded(ref threadedVoicePacket);
            }

            m_OutputQueue.Enqueue(threadedVoicePacket);
        }

        private static void ProcessOutput()
        {
            if (!IsEnabled)
                return;

            m_OutputMarker.MarkStart();

            while (m_OutputQueue != null && m_OutputQueue.TryDequeue(out var next))
                next?.Speaker?.Voice?.ReceiveThreaded(next);

            m_OutputMarker.MarkEnd();
        }

        private static void ProcessQueue()
        {
            while (m_RunThread)
            {
                m_ProcessMarker.MarkStart();

                if (!IsEnabled)
                {
                    Dispose();
                    break;
                }

                ProcessInput();

                m_ProcessMarker.MarkEnd();
            }
        }

        private static void InternalClean()
        {
            PrintStats();
            Clean();
        }

        private static void Info(object msg, string segment = null)
            => ApiLoader.Info($"Threaded Voice Chat{(segment != null ? $" / {segment}" : "")}", msg);

        private static void Warn(object msg, string segment = null)
            => ApiLoader.Warn($"Threaded Voice Chat{(segment != null ? $" / {segment}" : "")}", msg);

        private static void Error(object msg, string segment = null)
            => ApiLoader.Error($"Threaded Voice Chat{(segment != null ? $" / {segment}" : "")}", msg);

        private static void Debug(object msg, string segment = null)
            => ApiLoader.Debug($"Threaded Voice Chat{(segment != null ? $" / {segment}" : "")}", msg);
    }
}
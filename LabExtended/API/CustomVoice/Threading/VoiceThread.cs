using System.Collections.Concurrent;

using LabExtended.API.CustomVoice.Threading.Pitch;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Extensions;

using Mirror;

using VoiceChat.Codec;
using VoiceChat.Codec.Enums;

using VoiceChat.Networking;

namespace LabExtended.API.CustomVoice.Threading;

public delegate void SetupMessageHandler(ref VoiceMessage message);

public class VoiceThread : IDisposable
{
    private static volatile float globalPitch = 1f;

    public static float GlobalPitch
    {
        get => globalPitch;
        set => globalPitch = value;
    }

    private volatile bool isDisposed = false;
    private volatile float instancePitch = 1f;
    
    private volatile VoiceController voiceController;
    private volatile VoicePitchAction voicePitchAction;
    
    private volatile OpusEncoder opusEncoder;
    private volatile OpusDecoder opusDecoder;
    
    private volatile ConcurrentQueue<VoiceThreadPacket> inputQueue = new();
    private volatile ConcurrentQueue<VoiceThreadPacket> outputQueue = new();
    
    private volatile Action<VoiceThreadPacket> onPacketProcessed;

    public float InstancePitch
    {
        get => instancePitch;
        set => instancePitch = value;
    }

    public float ActivePitch
    {
        get
        {
            if (globalPitch != 1f)
                return globalPitch;
            
            return instancePitch;
        }
    }
    
    public bool IsDisposed => isDisposed;

    public VoiceThread(VoiceController controller)
    {
        voiceController = controller;
        voicePitchAction = new VoicePitchAction();

        opusDecoder = new OpusDecoder();
        opusEncoder = new OpusEncoder(OpusApplicationType.Voip);

        onPacketProcessed = ProcessPitched;

        Task.Run(UpdateInputQueue);

        StaticUnityMethods.OnFixedUpdate += UpdateOutputQueue;
        ExServerEvents.Quitting += Dispose;
    }

    public void Dispose()
    {
        if (isDisposed)
            return;

        StaticUnityMethods.OnFixedUpdate -= UpdateOutputQueue;
        ExServerEvents.Quitting -= Dispose;
        
        isDisposed = true;
        
        voiceController = null;

        voicePitchAction?.Dispose();
        voicePitchAction = null;

        opusDecoder?.Dispose();
        opusDecoder = null;
        
        opusEncoder?.Dispose();
        opusEncoder = null;
        
        inputQueue?.Clear();
        inputQueue = null;
        
        outputQueue?.Clear();
        outputQueue = null;
    }
    
    public void ProcessCustom<T>(byte[] originalData, int originalLength, IVoiceThreadAction action, Action<T> onProcessed, Func<T> packetFactory) where T : VoiceThreadPacket
    {
        if (originalData is null) throw new ArgumentNullException(nameof(originalData));
        if (originalLength < 0 || originalLength > originalData.Length) throw new ArgumentOutOfRangeException(nameof(originalData));
        if (action is null) throw new ArgumentNullException(nameof(action));
        if (onProcessed is null) throw new ArgumentNullException(nameof(onProcessed));
        if (packetFactory is null) throw new ArgumentNullException(nameof(packetFactory));
        
        var newBuffer = new byte[originalData.Length];
        
        Buffer.BlockCopy(originalData, 0, newBuffer, 0, originalLength);

        var newPacket = packetFactory();

        newPacket.Length = originalLength;
        newPacket.Data = newBuffer;

        newPacket.Action = action;
        newPacket.Speaker = voiceController.Player;

        newPacket.Decoder = opusDecoder;
        newPacket.Encoder = opusEncoder;

        newPacket.OnProcessed = packet => onProcessed((T)packet);
        
        inputQueue.Enqueue(newPacket);
    }

    public void ProcessCustom(byte[] originalData, int originalLength, IVoiceThreadAction action, Action<VoiceThreadPacket> onProcessed, Func<VoiceThreadPacket> packetFactory = null)
    {
        if (originalData is null) throw new ArgumentNullException(nameof(originalData));
        if (originalLength < 0 || originalLength > originalData.Length) throw new ArgumentOutOfRangeException(nameof(originalData));
        if (action is null) throw new ArgumentNullException(nameof(action));
        if (onProcessed is null) throw new ArgumentNullException(nameof(onProcessed));
        
        var newBuffer = new byte[originalData.Length];
        
        Buffer.BlockCopy(originalData, 0, newBuffer, 0, originalLength);

        var newPacket = packetFactory is null ? new VoiceThreadPacket() : packetFactory();

        newPacket.Length = originalLength;
        newPacket.Data = newBuffer;

        newPacket.Action = action;
        newPacket.Speaker = voiceController.Player;

        newPacket.Decoder = opusDecoder;
        newPacket.Encoder = opusEncoder;

        newPacket.OnProcessed = onProcessed;
        
        inputQueue.Enqueue(newPacket);
    }

    public void Pitch(byte[] originalData, int originalLength, float pitchFactor, Action<VoiceThreadPacket> onProcessed)
    {
        if (originalData is null) throw new ArgumentNullException(nameof(originalData));
        if (originalLength < 0 || originalLength > originalData.Length) throw new ArgumentOutOfRangeException(nameof(originalData));
        if (onProcessed is null) throw new ArgumentNullException(nameof(onProcessed));
        
        var newBuffer = new byte[originalData.Length];
        
        Buffer.BlockCopy(originalData, 0, newBuffer, 0, originalLength);

        var newPacket = new VoiceThreadPacket();

        newPacket.Length = originalLength;
        newPacket.Data = newBuffer;

        newPacket.Action = voicePitchAction;
        newPacket.Speaker = voiceController.Player;

        newPacket.Decoder = opusDecoder;
        newPacket.Encoder = opusEncoder;

        newPacket.OnProcessed = onProcessed;
        newPacket.Pitch = pitchFactor;
        
        inputQueue.Enqueue(newPacket);
    }

    public void Process(VoiceThreadPacket packet, SetupMessageHandler setupMessageHandler = null)
    {
        if (packet is null)
            throw new ArgumentNullException(nameof(packet));
        
        var newBuffer = new byte[packet.Data.Length];
        
        Buffer.BlockCopy(packet.Data, 0, newBuffer, 0, packet.Length);

        var newMessage = new VoiceMessage(voiceController.Player.ReferenceHub, packet.OriginalChannel, newBuffer, packet.Length, false);
        
        setupMessageHandler?.Invoke(ref newMessage);
        
        voiceController.ProcessMessage(ref newMessage);
    }

    private void ProcessPitch(ref VoiceMessage message)
    {
        var newBuffer = new byte[message.Data.Length];
        
        Buffer.BlockCopy(message.Data, 0, newBuffer, 0, message.DataLength);

        var newPacket = new VoiceThreadPacket();

        newPacket.OriginalChannel = message.Channel;
        newPacket.Length = message.DataLength;
        
        newPacket.Data = newBuffer;

        newPacket.Action = voicePitchAction;
        newPacket.Speaker = voiceController.Player;

        newPacket.Decoder = opusDecoder;
        newPacket.Encoder = opusEncoder;

        newPacket.OnProcessed = onPacketProcessed;
        newPacket.Pitch = ActivePitch;
        
        inputQueue.Enqueue(newPacket);
    }

    private void ProcessPitched(VoiceThreadPacket packet)
    {
        var newBuffer = new byte[packet.Data.Length];
        
        Buffer.BlockCopy(packet.Data, 0, newBuffer, 0, packet.Length);

        var newMessage = new VoiceMessage(voiceController.Player.ReferenceHub, packet.OriginalChannel, newBuffer, packet.Length, false);
        
        voiceController.ProcessMessage(ref newMessage);
    }

    internal void ProcessMessage(ref VoiceMessage msg)
    {
        if (!isDisposed)
        {
            if (!ApiLoader.ApiConfig.VoiceSection.DisableThreadedVoice && ActivePitch != 1f)
            {
                ProcessPitch(ref msg);
                return;
            }

            voiceController.ProcessMessage(ref msg);
        }
    }

    private void UpdateOutputQueue()
    {
        try
        {
            while (outputQueue.TryDequeue(out var packet) && ExServer.IsRunning && !isDisposed)
            {
                packet.OnProcessed.InvokeSafe(packet);
            }
        }
        catch (Exception ex)
        {
            ApiLog.Error("Voice Thread Output", ex);
        }
    }

    private void UpdateInputQueue()
    {
        while (!isDisposed && ExServer.IsRunning)
        {
            try
            {
                while (inputQueue.TryDequeue(out var packet) && ExServer.IsRunning)
                {
                    if (packet.Action is null)
                        continue;

                    packet.Action.Modify(ref packet);
                    
                    outputQueue.Enqueue(packet);
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("Voice Thread Input", ex);
            }
        }
    }

    public static VoiceMessage GetMessage(VoiceThreadPacket packet)
    {
        if (packet is null)
            throw new ArgumentNullException(nameof(packet));
        
        var newBuffer = new byte[packet.Data.Length];
        
        Buffer.BlockCopy(packet.Data, 0, newBuffer, 0, packet.Length);
        return new VoiceMessage(packet.Speaker.ReferenceHub, packet.OriginalChannel, newBuffer, packet.Length, false);
    }
}
using System.Collections.Concurrent;

using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Utilities.Unity;

using UnityEngine.PlayerLoop;

using VoiceChat.Codec;
using VoiceChat.Codec.Enums;
using VoiceChat.Networking;

namespace LabExtended.API.CustomVoice.Pitching;

public class VoicePitch : IDisposable
{
    public struct UpdateOutputQueueLoop { }
    
    static VoicePitch()
    {
        PlayerLoopHelper.ModifySystem(x => x.InjectBefore<TimeUpdate.WaitForLastPresentationAndUpdateTime>(UpdateOutputQueue, typeof(UpdateOutputQueueLoop)) ? x : null);
        ThreadPool.QueueUserWorkItem(_ => UpdateInputQueue());
    }
    
    private static volatile float globalPitch = 1f;
    
    private static volatile ConcurrentQueue<VoicePitchPacket> pitchQueue = new();
    private static volatile ConcurrentQueue<VoicePitchPacket> outputQueue = new();
    private static volatile ConcurrentQueue<VoicePitchPacket> packetPool = new();

    public static float GlobalPitch
    {
        get => globalPitch;
        set => globalPitch = value;
    }
    
    private volatile bool isDisposed;
    private volatile float instancePitch = 1f;
    
    private volatile VoiceController voiceController;
    private volatile VoicePitchAction voicePitchAction;
    
    private volatile OpusEncoder opusEncoder;
    private volatile OpusDecoder opusDecoder;
    
    private volatile Action<VoicePitchPacket> onVoicePitched;

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

    public VoicePitch(VoiceController controller)
    {
        voiceController = controller;
        
        voicePitchAction = new VoicePitchAction();
        voicePitchAction.voiceController = voiceController;

        opusDecoder = new OpusDecoder();
        opusEncoder = new OpusEncoder(OpusApplicationType.Voip);

        onVoicePitched = ProcessPitched;
    }

    public void Dispose()
    {
        voiceController = null;

        voicePitchAction?.Dispose();
        voicePitchAction = null;

        opusDecoder?.Dispose();
        opusEncoder?.Dispose();

        opusEncoder = null;
        opusDecoder = null;
    }

    public void ProcessCustom(byte[] originalData, int originalLength, IVoicePitchAction action, Action<VoicePitchPacket> onProcessed)
    {
        if (originalData is null)
            throw new ArgumentNullException(nameof(originalData));
        
        if (originalLength < 0 || originalLength > originalData.Length)
            throw new ArgumentOutOfRangeException(nameof(originalData));

        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        if (onProcessed is null)
            throw new ArgumentNullException(nameof(onProcessed));
        
        var newBuffer = new byte[originalData.Length];
        
        Buffer.BlockCopy(originalData, 0, newBuffer, 0, originalLength);

        var newPacket = GetPacket();

        newPacket.Length = originalLength;
        newPacket.Data = newBuffer;

        newPacket.Action = action;
        newPacket.Speaker = voiceController.Player;

        newPacket.Decoder = opusDecoder;
        newPacket.Encoder = opusEncoder;

        newPacket.OnProcessed = onProcessed;
        
        pitchQueue.Enqueue(newPacket);
    }

    private void ProcessPitch(ref VoiceMessage message)
    {
        var newBuffer = new byte[message.Data.Length];
        
        Buffer.BlockCopy(message.Data, 0, newBuffer, 0, message.DataLength);

        var newPacket = GetPacket();

        newPacket.Length = message.DataLength;
        newPacket.Data = newBuffer;

        newPacket.Action = voicePitchAction;
        newPacket.Speaker = voiceController.Player;

        newPacket.Decoder = opusDecoder;
        newPacket.Encoder = opusEncoder;

        newPacket.OnProcessed = onVoicePitched;
        
        pitchQueue.Enqueue(newPacket);
    }

    private void ProcessPitched(VoicePitchPacket packet)
    {
        var newBuffer = new byte[packet.Data.Length];
        
        Buffer.BlockCopy(packet.Data, 0, newBuffer, 0, packet.Length);

        var newMessage = new VoiceMessage(voiceController.Player.Hub, packet.OriginalChannel, newBuffer, packet.Length, false);
        
        voiceController.ProcessMessage(ref newMessage);
    }

    internal void ProcessMessage(ref VoiceMessage msg)
    {
        if (ActivePitch != 1f)
        {
            ProcessPitch(ref msg);
            return;
        }
        
        voiceController.ProcessMessage(ref msg);
    }

    private static void UpdateOutputQueue()
    {
        try
        {
            while (pitchQueue.TryDequeue(out var packet))
            {
                if (packet.OnProcessed is null)
                    continue;
                
                packet.OnProcessed.InvokeSafe(packet);
                
                packetPool.Enqueue(packet);
            }
        }
        catch (Exception ex)
        {
            ApiLog.Error("Voice Pitch Output", ex);
        }
    }

    private static void UpdateInputQueue()
    {
        while (true)
        {
            try
            {
                while (pitchQueue.TryDequeue(out var packet))
                {
                    if (packet.Action is null)
                        continue;

                    packet.Action.Modify(ref packet);
                    
                    outputQueue.Enqueue(packet);
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("Voice Pitch Input", ex);
            }
        }
    }

    private static VoicePitchPacket GetPacket()
    {
        if (!packetPool.TryDequeue(out var packet))
            return new VoicePitchPacket();

        return packet;
    }
    
    // default send code
    /*
            var sendChannel = voiceRole.VoiceModule.ValidateSend(msg.Channel);

            if (sendChannel is VoiceChatChannel.None)
                return false;

            voiceRole.VoiceModule.CurrentChannel = sendChannel;

            foreach (var hub in ReferenceHub.AllHubs)
            {
                if (hub.Mode != ClientInstanceMode.ReadyClient)
                    continue;

                if (hub.roleManager.CurrentRole is not IVoiceRole recvRole)
                    continue;

                var recvChannel = recvRole.VoiceModule.ValidateReceive(msg.Speaker, sendChannel);

                if (recvChannel is VoiceChatChannel.None)
                    continue;

                msg.Channel = recvChannel;
                hub.connectionToClient.Send(msg);
            }
     */
}
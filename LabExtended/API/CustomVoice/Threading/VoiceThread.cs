using System.Collections.Concurrent;

using LabExtended.API.CustomVoice.Threading.Pitch;
using LabExtended.Attributes;
using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Utilities.Update;

using Mirror;

using VoiceChat.Codec;
using VoiceChat.Codec.Enums;

using VoiceChat.Networking;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace LabExtended.API.CustomVoice.Threading;

/// <summary>
/// Used to modify voice messages on a background thread.
/// </summary>
public class VoiceThread : IDisposable
{
    private static volatile float globalPitch = 1f;
    
    private volatile ConcurrentQueue<VoiceThreadPacket> inputQueue = new();
    private volatile ConcurrentQueue<VoiceThreadPacket> outputQueue = new();

    /// <summary>
    /// Gets or sets the global voice pitch.
    /// </summary>
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
    
    private volatile Action<VoiceThreadPacket> onPacketProcessed;

    /// <summary>
    /// Gets or sets the pitch applied to this player.
    /// </summary>
    public float InstancePitch
    {
        get => instancePitch;
        set => instancePitch = value;
    }

    /// <summary>
    /// Gets the currently active voice pitch.
    /// </summary>
    public float ActivePitch
    {
        get
        {
            if (globalPitch != 1f)
                return globalPitch;
            
            return instancePitch;
        }
    }
    
    /// <summary>
    /// Whether or not this instance has been disposed.
    /// </summary>
    public bool IsDisposed => isDisposed;

    /// <summary>
    /// Creates a new <see cref="VoiceThread"/> instance.
    /// </summary>
    /// <param name="controller">The parent voice controller.</param>
    public VoiceThread(VoiceController controller)
    {
        if (controller is null)
            throw new ArgumentNullException(nameof(controller));
        
        voiceController = controller;
        voicePitchAction = new VoicePitchAction();

        opusDecoder = new OpusDecoder();
        opusEncoder = new OpusEncoder(OpusApplicationType.Voip);

        onPacketProcessed = ProcessPitched;
        
        ExServerEvents.Quitting += Dispose;
        PlayerUpdateHelper.OnUpdate += UpdateOutputQueue;
        
        Task.Run(UpdateInputQueueAsync);
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (isDisposed)
            return;

        PlayerUpdateHelper.OnUpdate -= UpdateOutputQueue;
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
    
    /// <summary>
    /// Adds a custom voice packet to be processed via the provided action.
    /// </summary>
    /// <param name="originalData">The data to modify.</param>
    /// <param name="originalLength">The original data length.</param>
    /// <param name="action">The action to invoke.</param>
    /// <param name="onProcessed">The method to call once the packet is processed.</param>
    /// <param name="packetFactory">The method used to create a new packet.</param>
    /// <typeparam name="T">The voice packet type.</typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void ProcessCustom<T>(byte[] originalData, int originalLength, IVoiceThreadAction action, Action<T> onProcessed, Func<T> packetFactory) 
        where T : VoiceThreadPacket
    {
        if (originalData is null) 
            throw new ArgumentNullException(nameof(originalData));
        
        if (originalLength < 0 || originalLength > originalData.Length) 
            throw new ArgumentOutOfRangeException(nameof(originalData));
        
        if (action is null) 
            throw new ArgumentNullException(nameof(action));
        
        if (onProcessed is null)
            throw new ArgumentNullException(nameof(onProcessed));
        
        if (packetFactory is null)
            throw new ArgumentNullException(nameof(packetFactory));
        
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

    /// <summary>
    /// Adds a custom voice packet to be processed via the provided action.
    /// </summary>
    /// <param name="originalData">The data to modify.</param>
    /// <param name="originalLength">The original data length.</param>
    /// <param name="action">The action to invoke.</param>
    /// <param name="onProcessed">The method to call once the packet is processed.</param>
    /// <param name="packetFactory">The method used to create a new packet.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void ProcessCustom(byte[] originalData, int originalLength, IVoiceThreadAction action, Action<VoiceThreadPacket> onProcessed,
        Func<VoiceThreadPacket> packetFactory = null)
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

    /// <summary>
    /// Applies pitch to the provided voice packet.
    /// </summary>
    /// <param name="originalData">The voice packet data.</param>
    /// <param name="originalLength">The voice packet data length.</param>
    /// <param name="pitchFactor">The pitch to apply.</param>
    /// <param name="onProcessed">The method to call once the pitch is processed.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Pitch(byte[] originalData, int originalLength, float pitchFactor, Action<VoiceThreadPacket> onProcessed)
    {
        if (originalData is null) 
            throw new ArgumentNullException(nameof(originalData));
        
        if (originalLength < 0 || originalLength > originalData.Length) 
            throw new ArgumentOutOfRangeException(nameof(originalData));
        
        if (onProcessed is null)
            throw new ArgumentNullException(nameof(onProcessed));
        
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
        
        inputQueue?.Enqueue(newPacket);
    }

    internal void ProcessPitch(ref VoiceMessage message)
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
        
        inputQueue?.Enqueue(newPacket);
    }

    private void ProcessPitched(VoiceThreadPacket packet)
    {
        var newBuffer = new byte[packet.Data.Length];
        
        Buffer.BlockCopy(packet.Data, 0, newBuffer, 0, packet.Length);

        var newMessage = new VoiceMessage(voiceController.Player.ReferenceHub, packet.OriginalChannel, newBuffer, packet.Length, false);
        
        voiceController?.ProcessMessage(ref newMessage);
    }

    private void UpdateOutputQueue()
    {
        try
        {
            var maxOutput = ApiLoader.ApiConfig.VoiceSection.MaxThreadOutput;
            var curOutput = 0;
            
            while (outputQueue != null && outputQueue.TryDequeue(out var packet))
            {
                packet.OnProcessed.InvokeSafe(packet);

                if (maxOutput > 0 && curOutput + 1 >= maxOutput)
                    break;

                curOutput++;
            }
        }
        catch (Exception ex)
        {
            ApiLog.Error("Voice Thread Output", ex);
        }
    }

    private async Task UpdateInputQueueAsync()
    {
        while (!isDisposed)
        {
            await Task.Delay(5);
            
            try
            {
                while (inputQueue != null && inputQueue.TryDequeue(out var packet))
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
}
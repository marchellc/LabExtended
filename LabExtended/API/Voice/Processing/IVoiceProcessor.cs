namespace LabExtended.API.Voice.Processing
{
    public interface IVoiceProcessor
    {
        bool IsGloballyActive { get; }

        bool ProcessData(ExPlayer speaker, ref byte[] data, ref int dataLength);

        bool IsActiveFor(ExPlayer player);
        bool SetActiveFor(ExPlayer player);
    }
}
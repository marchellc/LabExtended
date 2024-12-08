namespace LabExtended.Core.Ticking.Interfaces
{
    public interface ITickTimer : IDisposable
    {
        bool CanContinue();

        void OnExecuted();
    }
}
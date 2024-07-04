using LabExtended.API.Input.Interfaces;

namespace LabExtended.API.Input
{
    public class InputListener<TInfo> : IInputListener
        where TInfo : IInputInfo
    {
        public virtual void OnTriggered(TInfo info) { }

        public void Trigger(IInputInfo info)
        {
            if (info is null || info is not TInfo castInfo)
                return;

            OnTriggered(castInfo);
        }
    }
}
namespace LabExtended.Core.Hooking.Interfaces
{
    public interface IHookBinder
    {
        bool BindArgs(object eventObject, out object[] args);
        bool UnbindArgs(object[] args);
    }
}
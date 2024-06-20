namespace LabExtended.Core.Hooking.Interfaces
{
    public interface IHookRunner
    {
        void OnEvent(object eventObject, HookInfo hook, IHookBinder binder, Action<bool, bool, Exception, object> callback);
    }
}
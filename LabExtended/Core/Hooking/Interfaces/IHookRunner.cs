namespace LabExtended.Core.Hooking.Interfaces
{
    public interface IHookRunner
    {
        object OnEvent(object eventObject, HookInfo hook, IHookBinder binder);
    }
}
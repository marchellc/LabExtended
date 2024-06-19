namespace LabExtended.Core.Hooking
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Delegate | AttributeTargets.Event, AllowMultiple = false, Inherited = false)]
    public class HookIgnoreAttribute : Attribute { }
}
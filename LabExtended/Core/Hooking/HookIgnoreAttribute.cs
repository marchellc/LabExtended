namespace LabExtended.Core.Hooking
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class HookIgnoreAttribute : Attribute { }
}
namespace LabExtended.Core.Hooking
{
    public struct HookResult
    {
        public readonly object ReturnedValue;
        public readonly HookResultType Type;

        public HookResult(object returnValue, HookResultType hookResultType)
        {
            ReturnedValue = returnValue;
            Type = hookResultType;
        }
    }
}
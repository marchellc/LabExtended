namespace LabExtended.Core
{
    /// <summary>
    /// EntryPoint class for the ModuleInit package.
    /// </summary>
    public static class ModuleInitializer
    {
        /// <summary>
        /// EntryPoint method for the ModuleInit package.
        /// </summary>
        public static void Initialize()
            => ApiLoader.Load();
    }
}
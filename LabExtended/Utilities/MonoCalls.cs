using System.Runtime.InteropServices;

namespace LabExtended.Utilities
{
    /// <summary>
    /// A class that holds calls to the Mono runtime. Used by the Profiler.
    /// </summary>
    public static class MonoCalls
    {
        /// <summary>
        /// Get the approximate amount of memory used by managed objects.
        /// </summary>
        /// <returns>the amount of memory used in bytes</returns>
        [DllImport("__Internal")]
        public static extern long mono_gc_get_used_size();

        /// <summary>
        /// Get the amount of memory used by the garbage collector.
        /// </summary>
        /// <returns>the size of the heap in bytes</returns>
        [DllImport("__Internal")]
        public static extern long mono_gc_get_heap_size();
    }
}
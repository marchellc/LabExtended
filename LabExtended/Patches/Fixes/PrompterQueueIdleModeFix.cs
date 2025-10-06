using LabExtended.Core;
using LabExtended.Utilities.Update;

namespace LabExtended.Patches.Fixes
{
    /// <summary>
    /// Fixes the issue with the prompter queue not being processed while in idle mode.
    /// </summary>
    public static class PrompterQueueIdleModeFix
    {
        private static void Internal_Update()
        {
            if (IdleMode.IdleModeActive)
            {
                while (ServerConsole.PrompterQueue.TryDequeue(out var command))
                {
                    try
                    {
                        ServerConsole.EnterCommand(command, ServerConsole.Scs);
                    }
                    catch (Exception ex)
                    {
                        ApiLog.Error(nameof(PrompterQueueIdleModeFix), ex);
                    }
                }
            }
        }

        internal static void Internal_Init()
        {
            PlayerUpdateHelper.Component.OnUpdate += Internal_Update;
        }
    }
}
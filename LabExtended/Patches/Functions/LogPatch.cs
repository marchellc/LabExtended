﻿using HarmonyLib;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.AddLog))]
    public static class LogPatch
    {
        public static Harmony Harmony = new Harmony($"labextended.loader.patch.{DateTime.Now.Ticks}");

        public static void Enable()
            => Harmony.CreateClassProcessor(typeof(LogPatch)).Patch();

        public static event Action<string> OnLogging;

        public static bool Prefix(string q, ConsoleColor color)
        {
            if (string.IsNullOrWhiteSpace(q))
                return false;

            try
            {
                OnLogging?.Invoke(q);
            }
            catch { }

            ServerConsole.PrintOnOutputs(q, color);
            ServerConsole.PrintFormattedString(q, color);

            return false;
        }
    }
}
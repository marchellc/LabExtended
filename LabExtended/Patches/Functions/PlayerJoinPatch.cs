using CentralAuth;

using HarmonyLib;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Modules;
using LabExtended.Extensions;
using LabExtended.API.Modules;
using LabExtended.Hints;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.FinalizeAuthentication))]
    public static class PlayerJoinPatch
    {
        public static void Postfix(PlayerAuthenticationManager __instance)
        {
            try
            {
                if (__instance.isLocalPlayer)
                    return;

                var player = new ExPlayer(__instance._hub);

                if (TransientModule._cachedModules.TryGetValue(player.UserId, out var transientModules))
                {
                    foreach (var module in transientModules)
                    {
                        var type = module.GetType();

                        if (!player._modules.ContainsKey(type))
                        {
                            player._modules.Add(type, new ModuleContainer(module));

                            module.Parent = player;
                            module.IsActive = true;

                            module.Start();

                            ExLoader.Debug("Modules API", $"Re-added transient module &3{type.Name}&r (&6{module.ModuleId}&r) to player &3{player.Name}&r (&6{player.UserId}&r)!");
                        }
                        else
                        {
                            ExLoader.Warn("Extended API", $"Could not add transient module &3{type.Name}&r to player &3{player.Name}&r (&6{player.UserId}&r) - active instance found.");
                        }
                    }
                }

                player._hints = player.AddModule<HintModule>();

                if (player._modules.TryGetValue(typeof(PlayerStorageModule), out var moduleContainer))
                    player._storage = (PlayerStorageModule)moduleContainer.Module;
                else
                    player._storage = player.AddModule<PlayerStorageModule>();

                ExPlayer._players.Add(player);
                ExLoader.Info("Extended API", $"Player &3{player.Name}&r (&6{player.UserId}&r) &2joined&r from &3{player.Address}&r!");
            }
            catch (Exception ex)
            {
                ExLoader.Error("Extended API", $"An error occured while handling a player join!\n{ex.ToColoredString()}");
            }
        }
    }
}
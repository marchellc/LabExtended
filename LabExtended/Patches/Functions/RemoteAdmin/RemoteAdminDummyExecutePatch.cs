using CommandSystem;
using CommandSystem.Commands.RemoteAdmin.Dummies;

using HarmonyLib;
using LabExtended.API;
using LabExtended.API.RemoteAdmin.Actions;
using LabExtended.Extensions;

using NetworkManagerUtils.Dummies;

using Utils;

namespace LabExtended.Patches.Functions.RemoteAdmin;

/// <summary>
/// Implements custom Remote Admin actions.
/// </summary>
public static class RemoteAdminDummyExecutePatch
{
   [HarmonyPatch(typeof(ActionDummyCommand), nameof(ActionDummyCommand.Execute))]
   private static bool Prefix(ArraySegment<string> arguments, ICommandSender sender, ref bool __result, out string response)
   {
      if (!sender.CheckPermission(PlayerPermissions.FacilityManagement, out response))
         return __result = false;

      if (arguments.Count < 3)
      {
         response = "You must specify all arguments! (target, module, action)";
         return __result = false;
      }

      var hubs = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out _, false);

      if (hubs is null)
      {
         response = "An unexpected problem has occured during PlayerId or name array processing.";
         return __result = false;
      }

      var player = ExPlayer.Get(sender);

      var moduleName = arguments.At(1);
      var actionName = arguments.At(2);

      var count = 0;

      for (var i = 0; i < hubs.Count; i++)
      {
         var hub = hubs[i];
         
         if (hub == null)
            continue;

         var actions = DummyActionCollector.ServerGetActions(hub);
         var isCategory = false;

         for (var x = 0; x < actions.Count; x++)
         {
            var action = actions[x];
            var name = action.Name.Replace(' ', '_');

            if (action.Action is null)
            {
               isCategory = name == moduleName;
            }
            else if (isCategory && name == actionName)
            {
               if (action.Action.Target is RemoteAdminAction remoteAdminAction)
               {
                  remoteAdminAction.Invoke(player);
               }
               else
               {
                  action.Action.InvokeSafe();
               }

               count++;
            }
         }
      }

      response = $"Action requested on {count} dumm{(count == 1 ? "y!" : "ies!")}";

      __result = true;
      return false;
   }
}
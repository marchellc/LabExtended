using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.BasicMessages;
using InventorySystem.Items.Firearms.Modules;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;

using UnityEngine;

namespace LabExtended.Patches.Functions
{
    public static class AuthorizingShotPatch
    {
        public static bool AuthorizeShot(ExPlayer player, Firearm firearm, ShotMessage msg)
        {
            if (firearm.ActionModule is null || !firearm.ActionModule.Standby || player.Hub.HasBlock(BlockedInteraction.ItemPrimaryAction))
                return false;

            switch (firearm.ActionModule)
            {
                case AutomaticAction automaticAction:
                    return AutoAuthorizeShot(player, firearm, msg, automaticAction);

                case DisruptorAction disruptorAction:
                    return DisruptorAuthorizeShot(player, firearm, msg, disruptorAction);

                case DoubleAction doubleAction:
                    return DoubleAuthorizeShot(player, firearm, msg, doubleAction);

                case PumpAction pumpAction:
                    return PumpAuthorizeShot(player, firearm, msg, pumpAction);
            }

            return false;
        }

        private static bool AutoAuthorizeShot(ExPlayer player, Firearm firearm, ShotMessage msg, AutomaticAction action)
        {
            if (!action.ServerCheckFirerate())
                return false;

            var authorizingArgs = new PlayerAuthorizingShotArgs(player, firearm, player.Switches.HasUnlimitedAmmo ? byte.MinValue : (byte)action._ammoConsumption);

            if (!HookRunner.RunCancellable(authorizingArgs, true))
                return false;

            var flags = firearm.Status.Flags;

            if (authorizingArgs.SubstractAmmo > 0 && firearm.Status.Ammo - authorizingArgs.SubstractAmmo < authorizingArgs.SubstractAmmo && action._boltTravelTime == 0f)
                flags &= ~FirearmStatusFlags.Chambered;

            firearm.Status = new FirearmStatus((byte)Mathf.Clamp(firearm.Status.Ammo - authorizingArgs.SubstractAmmo, 0f, 255f), flags, firearm.Status.Attachments);
            firearm.ServerSendAudioMessage(action.ShotClipId);

            return true;
        }

        private static bool DisruptorAuthorizeShot(ExPlayer player, Firearm firearm, ShotMessage msg, DisruptorAction action)
        {
            if (!firearm.IsLocalPlayer && action.TimeSinceLastShot <= 1.5f)
                return false;

            var authorizingArgs = new PlayerAuthorizingShotArgs(player, firearm, player.Switches.HasUnlimitedAmmo ? byte.MinValue : (byte)1);

            if (!HookRunner.RunCancellable(authorizingArgs, true))
                return false;

            if (firearm.Status.Ammo == 0)
            {
                player.Hub.inventory.ServerRemoveItem(firearm.ItemSerial, null);
                return false;
            }

            if (authorizingArgs.SubstractAmmo > 0)
                firearm.Status = new FirearmStatus((byte)Mathf.Clamp(firearm.Status.Ammo - authorizingArgs.SubstractAmmo, 0f, 255f), firearm.Status.Flags, firearm.Status.Attachments);

            if (!firearm.IsLocalPlayer)
                action._lastShotTime = action.CurTime;

            firearm.ServerSideAnimator.Play(FirearmAnimatorHashes.Fire, 0, action.ShotDelay / 2.2667f);
            return true;
        }

        private static bool DoubleAuthorizeShot(ExPlayer player, Firearm firearm, ShotMessage msg, DoubleAction action)
        {
            if (action.ServerTriggerReady)
            {
                var authorizingArgs = new PlayerAuthorizingShotArgs(player, firearm, player.Switches.HasUnlimitedAmmo ? byte.MinValue : (byte)1);

                if (!HookRunner.RunCancellable(authorizingArgs, true))
                    return false;

                if (authorizingArgs.SubstractAmmo > 0 && (firearm.Status.Ammo - authorizingArgs.SubstractAmmo) < 0)
                    return false;

                if (authorizingArgs.SubstractAmmo > 0)
                    firearm.Status = new FirearmStatus((byte)Mathf.Clamp(firearm.Status.Ammo - authorizingArgs.SubstractAmmo, 0f, 255f), firearm.Status.Flags, firearm.Status.Attachments);

                action._nextAllowedShot = Time.timeSinceLevelLoad + action._cooldownAfterShot;
                firearm.ServerSendAudioMessage((byte)firearm.AttachmentsValue(AttachmentParam.ShotClipIdOverride));

                return true;
            }

            return false;
        }

        private static bool PumpAuthorizeShot(ExPlayer player, Firearm firearm, ShotMessage msg, PumpAction action)
        {
            if (action.ChamberedRounds == 0 || firearm.Status.Ammo == 0)
            {
                action.ServerResync();
                return false;
            }

            if (action._lastShotStopwatch.Elapsed.TotalSeconds < (double)action.TimeBetweenShots
                || action._pumpStopwatch.Elapsed.TotalSeconds < (double)action.PumpingTime)
                return false;

            var authorizingArgs = new PlayerAuthorizingShotArgs(player, firearm, player.Switches.HasUnlimitedAmmo ? byte.MinValue : (byte)action.AmmoUsage);

            if (!HookRunner.RunCancellable(authorizingArgs, true))
                return false;

            action.LastFiredAmount = 0;

            var flag = false;
            var ammo = action.AmmoUsage;

            while (ammo > 0 && firearm.Status.Ammo > 0)
            {
                ammo--;

                var chambered = action.ChamberedRounds;

                action.ChamberedRounds--;

                chambered = action.CockedHammers;

                action.CockedHammers--;
                action.LastFiredAmount = chambered + 1;

                if (action.LastFiredAmount > 0)
                    action._lastShotStopwatch.Restart();

                firearm.Status = new FirearmStatus((byte)Mathf.Clamp(firearm.Status.Ammo - authorizingArgs.SubstractAmmo, 0f, 255f), firearm.Status.Flags, firearm.Status.Attachments);
                firearm.ServerSendAudioMessage((byte)(action.ShotSoundId + action.ChamberedRounds));

                flag = true;

                if (action.ChamberedRounds == 0 && firearm.Status.Ammo > 0 && !firearm.IsLocalPlayer)
                {
                    action._pumpStopwatch.Restart();
                    firearm.AnimSetTrigger(action._pumpAnimHash);

                    break;
                }
            }

            return flag;
        }
    }
}
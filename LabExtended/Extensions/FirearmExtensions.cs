using Common.Extensions;

using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.BasicMessages;
using LabExtended.API;

namespace LabExtended.Extensions
{
    public static class FirearmExtensions
    {
        public static bool HasAttachment(this Firearm firearm, AttachmentName attachment)
            => firearm.Attachments.Any(a => a.Name == attachment && a.IsEnabled);

        public static bool EnableAttachments(this Firearm firearm, params AttachmentName[] attachments)
        {
            var anyChanged = false;

            foreach (var attachment in attachments)
            {
                var attachmentIndex = firearm.Attachments.FindIndex(a => a.Name == attachment);

                if (attachmentIndex < 0)
                    continue;

                if (firearm.Attachments[attachmentIndex].IsEnabled)
                    continue;

                firearm.Attachments[attachmentIndex].IsEnabled = true;
                anyChanged = true;
            }

            if (anyChanged)
                firearm.Status = new FirearmStatus(firearm.Status.Ammo, firearm.Status.Flags, firearm.ValidateAttachmentsCode(firearm.GetCurrentAttachmentsCode()));

            return anyChanged;
        }

        public static bool DisableAttachments(this Firearm firearm, params AttachmentName[] attachments)
        {
            var anyChanged = false;

            foreach (var attachment in attachments)
            {
                var attachmentIndex = firearm.Attachments.FindIndex(a => a.Name == attachment);

                if (attachmentIndex < 0)
                    continue;

                if (!firearm.Attachments[attachmentIndex].IsEnabled)
                    continue;

                firearm.Attachments[attachmentIndex].IsEnabled = false;
                anyChanged = true;
            }

            if (anyChanged)
                firearm.Status = new FirearmStatus(firearm.Status.Ammo, firearm.Status.Flags, firearm.ValidateAttachmentsCode(firearm.GetCurrentAttachmentsCode()));

            return anyChanged;
        }

        public static bool ToggleAttachments(this Firearm firearm, params AttachmentName[] attachments)
        {
            var anyChanged = false;

            foreach (var attachment in attachments)
            {
                var attachmentIndex = firearm.Attachments.FindIndex(a => a.Name == attachment);

                if (attachmentIndex < 0)
                    continue;

                firearm.Attachments[attachmentIndex].IsEnabled = !firearm.Attachments[attachmentIndex].IsEnabled;
                anyChanged = true;
            }

            if (anyChanged)
                firearm.Status = new FirearmStatus(firearm.Status.Ammo, firearm.Status.Flags, firearm.ValidateAttachmentsCode(firearm.GetCurrentAttachmentsCode()));

            return anyChanged;
        }

        public static IEnumerable<AttachmentName> GetActiveAttachments(this Firearm firearm)
            => firearm.Attachments.Where(a => a.IsEnabled).Select(a => a.Name);

        public static bool HasFlashlightEnabled(this Firearm firearm)
            => firearm.HasAdvantageFlag(AttachmentDescriptiveAdvantages.Flashlight) && firearm.Status.Flags.HasFlagFast(FirearmStatusFlags.FlashlightEnabled);

        public static bool HasNightVisionEnabled(this Firearm firearm)
            => firearm.IsAiming() && firearm.HasAdvantageFlag(AttachmentDescriptiveAdvantages.NightVision);

        public static void EnableFlashlight(this Firearm firearm, bool addIfMissing = false)
        {
            if (!firearm.HasAdvantageFlag(AttachmentDescriptiveAdvantages.Flashlight) && addIfMissing)
                firearm.EnableAttachments(AttachmentName.Flashlight);

            if (!firearm.Status.Flags.HasFlagFast(FirearmStatusFlags.FlashlightEnabled))
                firearm.Status = new FirearmStatus(firearm.Status.Ammo, firearm.Status.Flags | FirearmStatusFlags.FlashlightEnabled, firearm.Status.Attachments);
        }

        public static void DisableFlashlight(this Firearm firearm)
        {
            if (!firearm.Status.Flags.HasFlagFast(FirearmStatusFlags.FlashlightEnabled))
                return;

            firearm.Status = new FirearmStatus(firearm.Status.Ammo, firearm.Status.Flags & ~FirearmStatusFlags.FlashlightEnabled, firearm.Status.Attachments);
        }

        public static bool HasMagazineInserted(this Firearm firearm)
            => firearm.Status.Flags.HasFlagFast(FirearmStatusFlags.MagazineInserted);

        public static void RemoveMagazine(this Firearm firearm)
        {
            if (!firearm.Status.Flags.HasFlagFast(FirearmStatusFlags.MagazineInserted))
                return;

            firearm.Status = new FirearmStatus(firearm.Status.Ammo, firearm.Status.Flags & ~FirearmStatusFlags.MagazineInserted, firearm.Status.Attachments);
        }

        public static void InsertMagazine(this Firearm firearm)
        {
            if (firearm.Status.Flags.HasFlagFast(FirearmStatusFlags.MagazineInserted))
                return;

            firearm.Status = new FirearmStatus(firearm.Status.Ammo, firearm.Status.Flags | FirearmStatusFlags.MagazineInserted, firearm.Status.Attachments);
        }

        public static bool IsAiming(this Firearm firearm)
            => firearm.AdsModule.ServerAds;

        public static bool IsAutomatic(this Firearm firearm)
            => firearm is AutomaticFirearm;

        public static float GetFireRate(this Firearm firearm)
            => firearm is AutomaticFirearm automaticFirearm ? automaticFirearm._fireRate : 1f;

        public static void SetFireRate(this Firearm firearm, float value)
            => (firearm as AutomaticFirearm)!._fireRate = value;

        public static void ApplyPreferences(this Firearm firearm, ExPlayer player, bool randomizeIfNoPreferences = true)
        {
            if (!AttachmentsServerHandler.PlayerPreferences.TryGetValue(player.Hub, out var preferences) || !preferences.TryGetValue(firearm.ItemTypeId, out var preferenceCode))
            {
                if (randomizeIfNoPreferences)
                    preferenceCode = AttachmentsUtils.GetRandomAttachmentsCode(firearm.ItemTypeId);
                else
                    return;
            }

            firearm.ApplyAttachmentsCode(preferenceCode, true);
        }
    }
}
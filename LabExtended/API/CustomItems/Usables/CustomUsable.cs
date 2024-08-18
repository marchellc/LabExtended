using InventorySystem.Items.Usables;

using LabExtended.Core.Ticking;

using UnityEngine;

namespace LabExtended.API.CustomItems.Usables
{
    public class CustomUsable : CustomItem
    {
        static CustomUsable()
            => TickManager.OnTick += UpdateUsables;

        public UsableItem UsableItem
        {
            get
            {
                if (Item is null || Item is not UsableItem usableItem)
                    return null;

                return usableItem;
            }
        }

        public CustomUsableInfo UsableInfo
        {
            get
            {
                if (Info is null || Info is not CustomUsableInfo usableInfo)
                    return null;

                return usableInfo;
            }
        }

        public float UseTime => UsableInfo.UseTime;
        public float CooldownTime => UsableInfo.CooldownTime;

        public bool IsUsing { get; internal set; }

        public float RemainingTime { get; internal set; }
        public float RemainingCooldown { get; internal set; }

        public virtual void OnFinishedCooldown() { }
        public virtual void OnEnteredCooldown() { }

        public virtual void OnUsed() { }
        public virtual void OnUsedInCooldown() { }

        public virtual void OnUsing() { }

        public virtual void OnCancelled(CustomUsabeCancelReason reason) { }

        public virtual void UpdateUsing() { }
        public virtual void UpdateCooldown() { }

        public virtual bool ValidateUse()
            => true;

        public virtual bool ValidateCancel()
            => true;

        private static void UpdateUsables()
        {
            foreach (var customItem in ActiveItems.Values)
            {
                if (customItem is not CustomUsable customUsable || !customUsable.IsInInventory || customUsable.IsUsing)
                    continue;

                if (customUsable.RemainingCooldown == 0f)
                    continue;

                customUsable.RemainingCooldown -= Time.deltaTime;

                if (customUsable.RemainingCooldown <= 0f)
                {
                    customUsable.RemainingCooldown = 0f;
                    customUsable.OnFinishedCooldown();

                    continue;
                }

                customUsable.UpdateCooldown();
            }
        }
    }
}
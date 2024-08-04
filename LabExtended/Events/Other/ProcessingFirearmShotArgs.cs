using InventorySystem.Items.Firearms;

using LabExtended.API;
using LabExtended.Core.Events;

using UnityEngine;

namespace LabExtended.Events.Other
{
    public class ProcessingFirearmShotArgs : BoolCancellableEvent
    {
        public ExPlayer Player { get; }

        public Firearm Firearm { get; }

        public Ray Ray { get; set; }

        internal ProcessingFirearmShotArgs(ExPlayer player, Firearm firearm, Ray ray) => (Player, Firearm, Ray) = (player, firearm, ray);
    }
}
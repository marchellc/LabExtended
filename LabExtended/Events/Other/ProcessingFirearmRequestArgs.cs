using InventorySystem.Items.Firearms.BasicMessages;

using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Other
{
    public class ProcessingFirearmRequestArgs : BoolCancellableEvent
    {
        public ExPlayer Player { get; }

        public RequestMessage Message { get; set; }

        internal ProcessingFirearmRequestArgs(ExPlayer player, RequestMessage message)
        {
            Player = player;
            Message = message;
        }
    }
}
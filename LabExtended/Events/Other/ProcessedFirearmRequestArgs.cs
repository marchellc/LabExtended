using InventorySystem.Items.Firearms.BasicMessages;

using LabExtended.API;
using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Events.Other
{
    public class ProcessedFirearmRequestArgs : IHookEvent
    {
        public ExPlayer Player { get; }

        public RequestMessage Message { get; }

        internal ProcessedFirearmRequestArgs(ExPlayer player, RequestMessage msg)
        {
            Player = player;
            Message = msg;
        }
    }
}
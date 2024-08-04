using InventorySystem.Items.Firearms.BasicMessages;

using LabExtended.API;

namespace LabExtended.Events.Other
{
    public class ProcessedFirearmRequestArgs
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
using LabExtended.API.Items;

namespace LabExtended.API
{
    public class InventoryManager
    {
        private readonly ExPlayer _player;
        private readonly ReferenceHub _hub;
        private readonly BaseItem[] _items;

        public InventoryManager(ExPlayer player)
        {
            _player = player;
            _hub = player.ReferenceHub;
            _items = new BaseItem[8];
        }

        public IEnumerable<BaseItem> Items => _items;
    }
}
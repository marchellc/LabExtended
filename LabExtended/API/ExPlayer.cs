using Common.Extensions;

using LabExtended.API.Modules;

using PluginAPI.Core;

namespace LabExtended.API
{
    public class ExPlayer : Player
    {
        private static readonly List<ExPlayer> _players = new List<ExPlayer>();

        public static IReadOnlyList<ExPlayer> Players => _players;

        private ReferenceHub _hub;
        private ModuleParent _modules;
        private InventoryManager _inventory;

        public ExPlayer(ReferenceHub component) : base(component)
        {
            _hub = component;
            _modules = new ModuleParent();

            _inventory = new InventoryManager(this);
        }

        public ModuleParent Modules => _modules;
        public InventoryManager Inventory => _inventory;

        internal static void HandleJoin(ReferenceHub joinedHub)
        {
            var player = new ExPlayer(joinedHub);

            _players.Add(player);
        }

        internal static void HandleLeave(ReferenceHub leftHub)
        {
            if (!_players.TryGetFirst(pl => pl.ReferenceHub == leftHub, out var player))
                return;

            player._modules.StopModules();
            _players.Remove(player);
        }
    }
}
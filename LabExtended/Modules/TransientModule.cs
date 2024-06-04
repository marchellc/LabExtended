using Common.IO.Collections;

using LabExtended.API;

namespace LabExtended.Modules
{
    /// <summary>
    /// A module that is reused once the targeted player re-joins the server.
    /// </summary>
    public class TransientModule : Module
    {
        private static readonly LockedDictionary<string, List<TransientModule>> _modules = new LockedDictionary<string, List<TransientModule>>();

        /// <summary>
        /// Gets the player that owns this module.
        /// </summary>
        public ExPlayer Player { get; private set; }

        /// <summary>
        /// When overriden, returns a value indicating whether or not to keep this module instance after the player that owns it leaves.
        /// </summary>
        /// <returns>A value indicating whether or not to keep this module instance after the player that owns it leaves.</returns>
        public virtual bool ShouldKeepModule()
            => false;

        internal void Create(ExPlayer player)
        {
            Player = player;

            player.AddInstance(this, true);

            if (!_modules.TryGetValue(player.UserId, out var transientModules))
                transientModules = _modules[player.UserId] = new List<TransientModule>();

            if (!transientModules.Contains(this))
                transientModules.Add(this);
        }

        internal void Destroy()
        {
            var id = Player.UserId;

            Player.RemoveInstance(this, true);
            Player = null;

            if (!_modules.TryGetValue(id, out var moduleList))
                moduleList = _modules[id] = new List<TransientModule>();

            if (!ShouldKeepModule())
                moduleList.Remove(this);
        }

        internal static void Clear()
            => _modules.Clear();

        internal static List<TransientModule> Get(ExPlayer player)
            => _modules.TryGetValue(player.UserId, out var transientModules) ? transientModules : null;
    }
}
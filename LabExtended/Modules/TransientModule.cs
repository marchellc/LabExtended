using Common.IO.Collections;
using Common.Utilities;

using LabExtended.API;
using LabExtended.Core;

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
        public ExPlayer Player { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the owner is offline or not.
        /// </summary>
        public bool IsOffline => Player?.GameObject is null;

        /// <summary>
        /// Gets the module's ID.
        /// </summary>
        public string ModuleId { get; } = Generator.Instance.GetString(10);

        /// <summary>
        /// When overriden, returns a value indicating whether or not to keep this module instance after the player that owns it leaves.
        /// </summary>
        /// <returns>A value indicating whether or not to keep this module instance after the player that owns it leaves.</returns>
        public virtual bool ShouldKeepModule()
            => false;

        internal void Create(ExPlayer player, bool startModule = true)
        {
            Player = player;

            player.AddInstance(this, startModule);

            if (!_modules.TryGetValue(player.UserId, out var transientModules))
                transientModules = _modules[player.UserId] = new List<TransientModule>();

            if (!transientModules.Contains(this))
                transientModules.Add(this);
        }

        internal void Destroy(bool stopModule = true, bool removeModule = true)
        {
            var id = Player.UserId;

            Player.RemoveInstance(this, stopModule, removeModule);
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
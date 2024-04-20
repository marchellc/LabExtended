using Common.IO.Collections;
using Common.Values;

using PluginAPI.Core;

using UnityEngine;

namespace LabExtended.API;

public class ExPlayer : IWrapper<ReferenceHub>
{
	private static readonly LockedDictionary<Player, ExPlayer> _players = new LockedDictionary<Player, ExPlayer>();
	private static readonly LockedDictionary<GameObject, ExPlayer> _objects = new LockedDictionary<GameObject, ExPlayer>();
	private static readonly LockedDictionary<ReferenceHub, ExPlayer> _hubs = new LockedDictionary<ReferenceHub, ExPlayer>();

	private static readonly LockedDictionary<string, ExPlayer> _userIds = new LockedDictionary<string, ExPlayer>();
	private static readonly LockedDictionary<uint, ExPlayer> _netIds = new LockedDictionary<uint, ExPlayer>();
	private static readonly LockedDictionary<int, ExPlayer> _playerIds = new LockedDictionary<int, ExPlayer>();
	private static readonly LockedDictionary<int, ExPlayer> _connIds = new LockedDictionary<int, ExPlayer>();

	private ReferenceHub _hub;
	
	public ExPlayer(ReferenceHub hub)
	{
		if (hub is null)
			throw new ArgumentNullException(nameof(hub));
        
		var player = Player.Get(hub) ?? new Player(hub);

		_players[player] = this;
		
		_objects[hub.gameObject] = this;
		_hubs[hub] = this;

		_userIds[hub.authManager.UserId] = this;
		_playerIds[hub._playerId.Value] = this;
		_netIds[hub.netId] = this;

		if (hub.connectionToClient != null)
			_connIds[hub.connectionToClient.connectionId] = this;
	}
	
	public ExPlayer(Player player) : this(player.ReferenceHub) 
	{ }
	
	public ExPlayer(GameObject gameObject) : this(ReferenceHub.GetHub(gameObject))
	{ }

	public ReferenceHub Base
	{
		get => _hub;
		set => _hub = value;
	}
}
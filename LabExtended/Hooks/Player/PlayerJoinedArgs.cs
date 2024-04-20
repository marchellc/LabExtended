using LabExtended.API;
using LabExtended.Core.Hooking;

namespace LabExtended.Hooks.Player;

public class PlayerJoinedArgs : HookEvent
{
	public ExPlayer Player { get; }

	public PlayerJoinedArgs(ExPlayer player)
		=> Player = player;
}
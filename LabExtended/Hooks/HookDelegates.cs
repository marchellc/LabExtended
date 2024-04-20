using LabExtended.Hooks.Player;

namespace LabExtended.Hooks;

public static class HookDelegates
{
	public static event Action<PlayerJoiningArgs> OnPlayerJoining;
	public static event Action<PlayerJoinedArgs> OnPlayerJoined;
}
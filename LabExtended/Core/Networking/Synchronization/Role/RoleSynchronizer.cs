using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Utilities.Update;

using PlayerRoles;

namespace LabExtended.Core.Networking.Synchronization.Role;

/// <summary>
/// Sends role synchronization messages to players.
/// </summary>
public static class RoleSynchronizer
{
    private static void OnUpdate()
    {
        if (ExPlayer.AllPlayers.Count < 1)
            return;

        for (var i = 0; i < ExPlayer.AllPlayers.Count; i++)
        {
            var target = ExPlayer.AllPlayers[i];

            if (target?.Role is null || target.SentRoles is null)
                continue;

            for (var x = 0; x < ExPlayer.AllPlayers.Count; x++)
            {
                var receiver = ExPlayer.AllPlayers[x];
                var role = target.Role.Type;

                if (receiver is null || !receiver || receiver.Role is null)
                    continue;

                if (target.Role.Role is IObfuscatedRole obfuscatedRole)
                    role = obfuscatedRole.GetRoleForUser(receiver.ReferenceHub);

                if (target.Role.FakedList.HasGlobalValue)
                    role = target.Role.FakedList.GlobalValue;

                if (target.Role.FakedList.TryGetValue(receiver, out var fakedRole))
                    role = fakedRole;

                if (!receiver.Role.IsAlive && !target.Toggles.IsVisibleInSpectatorList)
                    role = RoleTypeId.Spectator;

                if (target.SentRoles.TryGetValue(receiver.NetworkId, out var sentRole) && sentRole == role)
                    continue;

                target.SentRoles[receiver.NetworkId] = role;
                
                receiver.Connection.Send(new RoleSyncInfo(target.ReferenceHub, role, receiver.ReferenceHub));
            }
        }
    }

    [LoaderInitialize(2)]
    private static void OnInit()
        => PlayerUpdateHelper.OnUpdate += OnUpdate;
}
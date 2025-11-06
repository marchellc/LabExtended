using LabExtended.API;
using LabExtended.API.Custom.Roles;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.CustomRoles
{
    /// <summary>
    /// Provides functionality for managing custom roles using server-side commands.
    /// </summary>
    [Command("customroles", "Manages custom roles.", "cr")]
    public class CustomRolesCommand : CommandBase, IServerSideCommand
    {
        [CommandOverload("list", "Lists all registered custom roles.", null)]
        private void List()
        {
            if (CustomRole.RegisteredObjects.Count == 0)
            {
                Fail($"No Custom Roles have been registered.");
                return;
            }

            Ok(x =>
            {
                x.AppendLine();

                foreach (var role in CustomRole.RegisteredObjects)
                    x.AppendLine($"[{role.Value.Id}] {role.Value.Name} ({role.Value.GetType().FullName})");
            });
        }

        [CommandOverload("active", "Displays the currently active custom role of a player.", null)]
        private void Active(ExPlayer? target = null)
        {
            target ??= Sender;

            if (target.Role.CustomRole is null)
            {
                Fail($"Player {target.ToCommandString()} does not have any active custom roles.");
                return;
            }

            Ok($"Player {target.ToCommandString()} has the '{target.Role.CustomRole.Name}' ({target.Role.CustomRole.Id}) custom role active.");
        }

        [CommandOverload("set", "Sets the custom role of a player.", null)]
        private void Set(ExPlayer target, string roleId)
        {
            if (!CustomRole.TryGet(roleId, out var role))
            {
                Fail($"Custom Role with ID '{roleId}' does not exist.");
                return;
            }

            if (target.Role.CustomRole == role)
            {
                Fail($"Player {target.ToLogString()} already has the '{role.Name}' ({role.Id}) custom role active.");
                return;
            }

            if (role.Give(target))
                Ok($"Set the '{role.Name}' ({role.Id}) custom role for player {target.ToCommandString()}.");
            else
                Fail($"Could not set custom role '{role.Name}' ({role.Id}) to player {target.ToCommandString()}");
        }

        [CommandOverload("remove", "Removes the custom role of a player.", null)]
        private void Remove(ExPlayer? target = null)
        {
            target ??= Sender;

            if (target.Role.CustomRole is null)
            {
                Fail($"Player {target.ToLogString()} does not have any active custom roles.");
                return;
            }

            var role = target.Role.CustomRole;

            if (target.Role.CustomRole.Remove(target))
                Ok($"Removed custom role '{role.Name}' ({role.Id}) from player {target.ToCommandString()}");
            else
                Fail($"Could not remove custom role '{role.Name}' ({role.Id}) from player {target.ToCommandString()}");
        }
    }
}
using CommandSystem;

using LabExtended.API.Prefabs;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using NorthwoodLib.Pools;

namespace LabExtended.API.CustomCommands.ListCommands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class ListPrefabsCommand : CustomCommand
    {
        public override string Command => "listprefabs";
        public override string Description => "Lists all available prefabs.";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            var builder = StringBuilderPool.Shared.Rent();
            var prefabs = PrefabList.AllPrefabs;

            builder.AppendLine($"Loaded {prefabs.Count} prefabs:");

            foreach (var prefab in prefabs)
                builder.AppendLine($"- {prefab.Key} ({prefab.Value.GameObject?.name ?? "null"})");

            ctx.RespondOk(StringBuilderPool.Shared.ToStringReturn(builder));
        }
    }
}
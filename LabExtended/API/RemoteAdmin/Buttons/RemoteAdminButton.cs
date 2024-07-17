using LabExtended.API.Collections.Locked;
using LabExtended.API.RemoteAdmin.Enums;
using LabExtended.API.RemoteAdmin.Interfaces;

using LabExtended.Utilities;
using NorthwoodLib.Pools;
using System.Text;

namespace LabExtended.API.RemoteAdmin.Buttons
{
    public class RemoteAdminButton : IRemoteAdminButton
    {
        private readonly LockedHashSet<IRemoteAdminObject> _binds = new LockedHashSet<IRemoteAdminObject>();
        private readonly RemoteAdminButtonType _type;

        public RemoteAdminButton(RemoteAdminButtonType type)
            => _type = type;

        public void OnOpened(ExPlayer player, StringBuilder builder, int pos, List<IRemoteAdminObject> appendedNames)
        {
            for (int i = 0; i < _binds.Count; i++)
            {
                var bind = _binds[i];
                var size = 1;
                var line = bind.GetButton(player, _type);

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                HintUtils.TrimStartNewLines(ref line, out _);

                if (!HintUtils.TryGetSizeTag(line, out size, out _, out var closed))
                    size = 1;
                else
                {
                    if (!closed)
                        line += "</size>";
                }

                if (!appendedNames.Contains(bind))
                {
                    builder.Append($"<voffset=-{324 - ((i == 0 ? size : size + i) * 30)}><br><pos=78%>{bind.GetName(player)}</pos></voffset>");
                    appendedNames.Add(bind);
                }

                if (pos != 0)
                    builder.Append($"<voffset=-{(pos == 26 ? 326 : 329) - ((i == 0 ? size : size + i) * 30)}><br><pos={pos}%>{line}</pos></voffset>");
                else
                    builder.Append($"<voffset=-{324 - ((i == 0 ? size : size + i) * 30)}><br>{line}</voffset>");
            }
        }

        public bool OnPressed(ExPlayer player, IEnumerable<int> selectedObjects)
        {
            var players = ExPlayer.Get(p => selectedObjects.Contains(p.PlayerId));
            var objects = player.RemoteAdmin.Objects.Where(obj => selectedObjects.Contains(obj.ListId));

            if (players.Any() && !objects.Any())
                return true;

            var builder = StringBuilderPool.Shared.Rent();

            foreach (var obj in objects)
                builder.AppendLine(obj.GetResponse(player, players, _type));

            player.RemoteAdminInfo(StringBuilderPool.Shared.ToStringReturn(builder));
            return false;
        }

        public bool BindObject(IRemoteAdminObject remoteAdminObject)
        {
            if (remoteAdminObject is null)
                throw new ArgumentNullException(nameof(remoteAdminObject));

            return _binds.Add(remoteAdminObject);
        }

        public bool UnbindObject(IRemoteAdminObject remoteAdminObject)
        {
            if (remoteAdminObject is null)
                throw new ArgumentNullException(nameof(remoteAdminObject));

            return _binds.Remove(remoteAdminObject);
        }
    }
}
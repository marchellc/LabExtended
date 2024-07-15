using Common.Pooling.Pools;

using LabExtended.API.Collections.Locked;
using LabExtended.API.RemoteAdmin.Enums;
using LabExtended.API.RemoteAdmin.Interfaces;

using LabExtended.Utilities;

using System.Text;

namespace LabExtended.API.RemoteAdmin.Buttons
{
    public class RemoteAdminButton : IRemoteAdminButton
    {
        private readonly LockedHashSet<RemoteAdminButtonBind> _binds = new LockedHashSet<RemoteAdminButtonBind>();
        private readonly RemoteAdminButtonType _type;

        public RemoteAdminButton(RemoteAdminButtonType type)
            => _type = type;

        public void OnOpened(ExPlayer player, StringBuilder builder, int pos, List<IRemoteAdminObject> appendedNames)
        {
            for (int i = 0; i < _binds.Count; i++)
            {
                var bind = _binds[i];
                var size = 1;
                var line = bind.Name;

                if (string.IsNullOrWhiteSpace(bind.Name))
                    continue;

                HintUtils.TrimStartNewLines(ref line, out _);

                if (!HintUtils.TryGetSizeTag(bind.Name, ref size, out var closed))
                    size = 1;
                else
                {
                    if (!closed)
                        line += "</size>";
                }

                if (!appendedNames.Contains(bind.Object))
                {
                    builder.Append($"<voffset=-{324 - ((i == 0 ? size : size + i) * 30)}><br><pos=78%>{bind.Object.GetName(player)}</pos></voffset>");
                    appendedNames.Add(bind.Object);
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

        public bool BindObject(IRemoteAdminObject remoteAdminObject, string name = null)
        {
            if (remoteAdminObject is null)
                throw new ArgumentNullException(nameof(remoteAdminObject));

            if (_binds.Any(bind => bind.Object == remoteAdminObject))
                return false;

            return _binds.Add(new RemoteAdminButtonBind(remoteAdminObject, name));
        }

        public bool UnbindObject(IRemoteAdminObject remoteAdminObject)
        {
            if (remoteAdminObject is null)
                throw new ArgumentNullException(nameof(remoteAdminObject));

            return _binds.RemoveWhere(bind => bind.Object == remoteAdminObject) > 0;
        }
    }
}
using LabExtended.API.RemoteAdmin.Enums;
using LabExtended.API.RemoteAdmin.Interfaces;

using LabExtended.Utilities;

using NorthwoodLib.Pools;

using System.Text;

// ReSharper disable PossibleMultipleEnumeration

namespace LabExtended.API.RemoteAdmin.Buttons;

/// <summary>
/// A basic Remote Admin button.
/// </summary>
public class RemoteAdminButton : IRemoteAdminButton
{
    private readonly List<IRemoteAdminObject> _binds = new();
    private readonly RemoteAdminButtonType _type;

    internal RemoteAdminButton(RemoteAdminButtonType type)
        => _type = type;

    /// <inheritdoc cref="IRemoteAdminButton.OnOpened"/>
    public void OnOpened(ExPlayer player, StringBuilder builder, int pos, List<IRemoteAdminObject> appendedNames)
    {
        var index = 0;

        _binds.ForEach(bind =>
        {
            var size = 1;
            var line = bind.GetButton(player, _type);

            if (string.IsNullOrWhiteSpace(line))
            {
                index++;
                return;
            }

            HintUtils.TrimStartNewLines(ref line, out _);

            if (!HintUtils.TryGetSizeTag(line, out size, out _, out var closed))
            {
                size = 1;
            }
            else
            {
                if (!closed)
                {
                    line += "</size>";
                }
            }

            if (!appendedNames.Contains(bind))
            {
                builder.Append(
                    $"<voffset=-{324 - ((index == 0 ? size : size + index) * 30)}><br><pos=78%>{bind.GetName(player)}</pos></voffset>");
                appendedNames.Add(bind);
            }

            if (pos != 0)
                builder.Append(
                    $"<voffset=-{(pos == 26 ? 326 : 329) - ((index == 0 ? size : size + index) * 30)}><br><pos={pos}%>{line}</pos></voffset>");
            else
                builder.Append($"<voffset=-{324 - ((index == 0 ? size : size + index) * 30)}><br>{line}</voffset>");

            index++;
        });
    }

    /// <inheritdoc cref="IRemoteAdminButton.OnPressed"/>
    public bool OnPressed(ExPlayer player, IEnumerable<int> selectedObjects)
    {
        var players = ExPlayer.Get(p => selectedObjects.Contains(p.PlayerId));
        var objects = player.RemoteAdmin.Objects.Where(obj => selectedObjects.Contains(obj.ListId));
        
        if (players.Any() && !objects.Any())
            return true;

        var builder = StringBuilderPool.Shared.Rent();

        foreach (var obj in objects)
            builder.AppendLine(obj.GetResponse(player, players, _type));

        player.SendRemoteAdminInfo(StringBuilderPool.Shared.ToStringReturn(builder));
        return false;
    }

    /// <inheritdoc cref="IRemoteAdminButton.BindObject"/>
    public bool BindObject(IRemoteAdminObject remoteAdminObject)
    {
        if (remoteAdminObject is null)
            throw new ArgumentNullException(nameof(remoteAdminObject));

        if (_binds.Contains(remoteAdminObject))
            return false;

        _binds.Add(remoteAdminObject);
        return true;
    }

    /// <inheritdoc cref="IRemoteAdminButton.UnbindObject"/>
    public bool UnbindObject(IRemoteAdminObject remoteAdminObject)
    {
        if (remoteAdminObject is null)
            throw new ArgumentNullException(nameof(remoteAdminObject));

        return _binds.Remove(remoteAdminObject);
    }
}
using LabExtended.API.RemoteAdmin.Buttons;
using LabExtended.API.RemoteAdmin.Enums;
using LabExtended.API.RemoteAdmin.Interfaces;

namespace LabExtended.API.RemoteAdmin
{
    public static class RemoteAdminButtons
    {
        private static readonly Dictionary<RemoteAdminButtonType, IRemoteAdminButton> _buttons = new()
        {
            [RemoteAdminButtonType.Request] = new RemoteAdminButton(RemoteAdminButtonType.Request),
            [RemoteAdminButtonType.RequestIp] = new RemoteAdminButton(RemoteAdminButtonType.RequestIp),
            [RemoteAdminButtonType.RequestAuth] = new RemoteAdminButton(RemoteAdminButtonType.RequestAuth),
            [RemoteAdminButtonType.ExternalLookup] = new RemoteAdminButton(RemoteAdminButtonType.ExternalLookup)
        };

        public static IReadOnlyDictionary<RemoteAdminButtonType, IRemoteAdminButton> Buttons => _buttons;

        public static bool RegisterButton(RemoteAdminButtonType buttonType, IRemoteAdminButton button)
        {
            if (button is null)
                return false;

            _buttons[buttonType] = button;
            return true;
        }

        public static bool UnregisterButton(RemoteAdminButtonType buttonType)
            => _buttons.Remove(buttonType);

        public static bool TryGetButton(RemoteAdminButtonType buttonType, out IRemoteAdminButton remoteAdminButton)
            => _buttons.TryGetValue(buttonType, out remoteAdminButton);

        public static IRemoteAdminButton GetButton(RemoteAdminButtonType buttonType)
            => _buttons.TryGetValue(buttonType, out var remoteAdminButton) ? remoteAdminButton : null;

        public static void InvokeButton(RemoteAdminButtonType buttonType, ExPlayer player, IEnumerable<int> selectedObjects)
            => GetButton(buttonType)?.OnPressed(player, selectedObjects);

        public static bool BindButton(RemoteAdminButtonType buttonType, IRemoteAdminObject remoteAdminObject)
        {
            if (remoteAdminObject is null)
                throw new ArgumentNullException(nameof(remoteAdminObject));

            if (!TryGetButton(buttonType, out var button))
                return false;

            return button.BindObject(remoteAdminObject);
        }

        public static bool UnbindButton(RemoteAdminButtonType buttonType, IRemoteAdminObject remoteAdminObject)
        {
            if (remoteAdminObject is null)
                throw new ArgumentNullException(nameof(remoteAdminObject));

            if (!TryGetButton(buttonType, out var button))
                return false;

            return button.UnbindObject(remoteAdminObject);
        }
    }
}

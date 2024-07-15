﻿using LabExtended.API.Enums;
using LabExtended.API.RemoteAdmin.Enums;
using LabExtended.API.RemoteAdmin.Interfaces;

namespace LabExtended.API.RemoteAdmin.Objects
{
    public class DynamicRemoteAdminObject : IRemoteAdminObject
    {
        public DynamicRemoteAdminObject(Func<ExPlayer, string> name, Func<ExPlayer, IEnumerable<ExPlayer>, RemoteAdminButtonType, string> response, Func<ExPlayer, bool> visibility = null)
        {
            Name = name;
            Response = response;
            Visibility = visibility;
        }

        public Func<ExPlayer, string> Name { get; set; }
        public Func<ExPlayer, IEnumerable<ExPlayer>, RemoteAdminButtonType, string> Response { get; set; }

        public Func<ExPlayer, bool> Visibility { get; set; }

        public RemoteAdminObjectFlags Flags { get; set; } = RemoteAdminObjectFlags.ShowToNorthwoodStaff;
        public RemoteAdminIconType Icons { get; set; } = RemoteAdminIconType.None;

        public string CustomId { get; set; }

        public string Id { get; set; }
        public int ListId { get; set; }
        public bool IsActive { get; set; }

        public string GetName(ExPlayer player)
            => Name(player);

        public string GetResponse(ExPlayer player, IEnumerable<ExPlayer> selectedPlayers, RemoteAdminButtonType button)
            => Response(player, selectedPlayers, button);

        public bool GetVisiblity(ExPlayer player)
            => Visibility is null || Visibility(player);

        public virtual void OnDisabled() { }
        public virtual void OnEnabled() { }
    }
}
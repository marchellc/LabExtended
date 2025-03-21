﻿using LabExtended.API.Enums;
using LabExtended.API.RemoteAdmin.Enums;
using LabExtended.API.RemoteAdmin.Interfaces;

using LabExtended.Events;
using LabExtended.Extensions;

using LabExtended.Utilities.Update;
using LabExtended.Utilities.Generation;

using LabExtended.Patches.Functions.RemoteAdmin;

using NorthwoodLib.Pools;

using System.Text;

namespace LabExtended.API.RemoteAdmin
{
    public class RemoteAdminController : IDisposable
    {
        private static readonly UniqueStringGenerator _objectIdGenerator = new UniqueStringGenerator(10, false);
        private static readonly UniqueInt32Generator _listIdGenerator = new UniqueInt32Generator(6000, 11000);

        private static readonly HashSet<Type> _globalObjects = new();

        private List<IRemoteAdminObject> _objects = new();
        private DateTime _lastListRequestTime = DateTime.MinValue;
        private bool _wasOpen = false;

        public bool IsRemoteAdminOpen { get; private set; }
        
        public ExPlayer Player { get; }

        public IReadOnlyList<IRemoteAdminObject> Objects => _objects;

        public RemoteAdminController(ExPlayer player)
        {
            _lastListRequestTime = DateTime.MinValue;
            _wasOpen = false;
            
            Player = player;

            foreach (var type in _globalObjects)
                AddObject(type);

            PlayerUpdateHelper.OnUpdate += Update;
        }

        public void Dispose()
        {
            PlayerUpdateHelper.OnUpdate -= Update;
            
            _lastListRequestTime = DateTime.MinValue;
            _wasOpen = false;

            foreach (var obj in _objects)
            {
                if (obj.IsActive)
                {
                    obj.IsActive = false;
                    obj.OnDisabled();
                }

                _objectIdGenerator.Free(obj.Id);
                _listIdGenerator.Free(obj.ListId);
            }
        }

        public void SendObjectHelp()
        {
            if (_objects.Count < 1)
                return;

            var list = ListPool<IRemoteAdminObject>.Shared.Rent();
            var builder = StringBuilderPool.Shared.Rent();
            var pos = 0;

            builder.Append("<line-height=0>");

            foreach (var button in RemoteAdminButtons.Buttons)
            {
                button.Value.OnOpened(Player, builder, pos, list);
                pos += 26;
            }

            ListPool<IRemoteAdminObject>.Shared.Return(list);

            Player.SendRemoteAdminInfo(StringBuilderPool.Shared.ToStringReturn(builder));
        }

        public IRemoteAdminObject AddObject(Type objectType, string customId = null)
        {
            if (objectType is null)
                throw new ArgumentNullException(nameof(objectType));

            if (!objectType.InheritsType<IRemoteAdminObject>())
                throw new InvalidOperationException($"Type {objectType.FullName} does not inherit interface IRemoteAdminObject");

            if (!string.IsNullOrWhiteSpace(customId) && _objects.Any(obj => !string.IsNullOrWhiteSpace(obj.CustomId) && obj.CustomId == customId))
                throw new InvalidOperationException($"There's already an object with the same ID ({customId})");

            var raObject = objectType.Construct<IRemoteAdminObject>();

            raObject.CustomId = customId;

            raObject.Id = _objectIdGenerator.Next();
            raObject.ListId = _listIdGenerator.Next();

            raObject.IsActive = true;
            raObject.OnEnabled();

            _objects.Add(raObject);
            return raObject;
        }

        public T AddObject<T>(string customId = null) where T : IRemoteAdminObject
            => (T)AddObject(typeof(T), customId);

        public void AddObject(IRemoteAdminObject remoteAdminObject)
        {
            if (remoteAdminObject is null)
                throw new ArgumentNullException(nameof(remoteAdminObject));

            if (_objects.Contains(remoteAdminObject))
                throw new InvalidOperationException($"This object has already been added.");

            if (!string.IsNullOrWhiteSpace(remoteAdminObject.CustomId) && _objects.Any(obj => !string.IsNullOrWhiteSpace(obj.CustomId) && obj.CustomId == remoteAdminObject.CustomId))
                throw new InvalidOperationException($"There's already an object with the same ID ({remoteAdminObject.CustomId})");

            if (!remoteAdminObject.IsActive)
            {
                remoteAdminObject.OnEnabled();
                remoteAdminObject.IsActive = true;
            }

            if (string.IsNullOrWhiteSpace(remoteAdminObject.Id))
                remoteAdminObject.Id = _objectIdGenerator.Next();

            remoteAdminObject.ListId = _listIdGenerator.Next();

            _objects.Add(remoteAdminObject);
        }

        public bool RemoveObject(IRemoteAdminObject remoteAdminObject)
        {
            if (remoteAdminObject is null)
                throw new ArgumentNullException(nameof(remoteAdminObject));

            if (remoteAdminObject.IsActive)
            {
                remoteAdminObject.IsActive = false;
                remoteAdminObject.OnDisabled();
            }

            _objectIdGenerator.Free(remoteAdminObject.Id);
            _listIdGenerator.Free(remoteAdminObject.ListId);

            return _objects.Remove(remoteAdminObject);
        }

        public bool RemoveObject<T>() where T : IRemoteAdminObject
        {
            if (!TryGetObject<T>(out var remoteAdminObject))
                return false;

            return RemoveObject(remoteAdminObject);
        }

        public bool RemoveObject(string customId)
        {
            if (!TryGetObject(customId, out var remoteAdminObject))
                return false;

            return RemoveObject(remoteAdminObject);
        }

        public bool RemoveObject(int listId)
        {
            if (!TryGetObject(listId, out var remoteAdminObject))
                return false;

            return RemoveObject(remoteAdminObject);
        }

        public bool TryGetObject<T>(out T remoteAdminObject) where T : IRemoteAdminObject
            => _objects.TryGetFirst(out remoteAdminObject);

        public bool TryGetObject(string customId, out IRemoteAdminObject remoteAdminObject)
            => _objects.TryGetFirst(obj => !string.IsNullOrWhiteSpace(obj.CustomId) && obj.CustomId == customId, out remoteAdminObject);

        public bool TryGetObject(int listId, out IRemoteAdminObject remoteAdminObject)
            => _objects.TryGetFirst(obj => obj.ListId == listId, out remoteAdminObject);

        public bool TryGetObject<T>(string customId, out T remoteAdminObject) where T : IRemoteAdminObject
            => _objects.TryGetFirst(obj => !string.IsNullOrWhiteSpace(obj.CustomId) && obj.CustomId == customId, out remoteAdminObject);

        public bool TryGetObject<T>(int listId, out T remoteAdminObject) where T : IRemoteAdminObject
            => _objects.TryGetFirst(obj => obj.ListId == listId, out remoteAdminObject);

        internal void InternalPrependObjects(StringBuilder builder)
        {
            foreach (var obj in _objects)
            {
                if (!obj.IsActive) continue;
                if (!obj.Flags.Any(RemoteAdminObjectFlags.ShowOnTop)) continue;
                if (!obj.Flags.Any(RemoteAdminObjectFlags.ShowToNorthwoodStaff) && Player.IsNorthwoodStaff) continue;
                if (!obj.GetVisibility(Player)) continue;

                if (obj.Icons != RemoteAdminIconType.None)
                {
                    if ((obj.Icons & RemoteAdminIconType.MutedIcon) != 0) builder.Append(RemoteAdminListPatch.MutedIconPrefix);
                    if ((obj.Icons & RemoteAdminIconType.OverwatchIcon) != 0) builder.Append(RemoteAdminListPatch.OverwatchIconPrefix);
                }

                builder.Append($"({obj.ListId}) ");
                builder.Append(obj.GetName(Player).Replace("\n", string.Empty).Replace("RA_", string.Empty)).Append("</color>");
                builder.AppendLine();
            }
        }

        internal void InternalAppendObjects(StringBuilder builder)
        {
            foreach (var obj in _objects)
            {
                if (!obj.IsActive) continue;
                if (obj.Flags.Any(RemoteAdminObjectFlags.ShowOnTop)) continue;
                if (!obj.Flags.Any(RemoteAdminObjectFlags.ShowToNorthwoodStaff) && Player.IsNorthwoodStaff) continue;
                if (!obj.GetVisibility(Player)) continue;

                if (obj.Icons != RemoteAdminIconType.None)
                {
                    if ((obj.Icons & RemoteAdminIconType.MutedIcon) != 0) builder.Append(RemoteAdminListPatch.MutedIconPrefix);
                    if ((obj.Icons & RemoteAdminIconType.OverwatchIcon) != 0) builder.Append(RemoteAdminListPatch.OverwatchIconPrefix);
                }

                builder.Append($"({obj.ListId}) ");
                builder.Append(obj.GetName(Player).Replace("\n", string.Empty).Replace("RA_", string.Empty)).Append("</color>");
                builder.AppendLine();
            }
        }

        internal void InternalRegisterRequest()
            => _lastListRequestTime = DateTime.Now;
        
        private void Update()
        {
            IsRemoteAdminOpen = (DateTime.Now - _lastListRequestTime).TotalSeconds < 1.1 + Player.Ping;

            if (IsRemoteAdminOpen == _wasOpen) 
                return;
            
            _wasOpen = IsRemoteAdminOpen;

            if (IsRemoteAdminOpen)
                ExPlayerEvents.OnOpenedRemoteAdmin(new(Player));
            else
                ExPlayerEvents.OnClosedRemoteAdmin(new(Player));
        }
    }
}
using Common.Caching;
using Common.Extensions;
using Common.IO.Collections;
using Common.Utilities.Generation;

namespace LabExtended.API.RemoteAdmin
{
    /// <summary>
    /// A class used to help with managing the Remote Admin panel.
    /// </summary>
    public static class RemoteAdminUtils
    {
        private static readonly UniqueInt32Generator _idGenerator = new UniqueInt32Generator(new MemoryCache<int>(), 5000, 8000);
        private static readonly LockedList<RemoteAdminObject> _raObjects = new LockedList<RemoteAdminObject>();

        /// <summary>
        /// Gets a list of all fake player list objects.
        /// </summary>
        public static IEnumerable<RemoteAdminObject> AdditionalObjects => _raObjects;

        /// <summary>
        /// Tries to add a new remote admin object.
        /// </summary>
        /// <param name="remoteAdminPlayerObject">The object to add.</param>
        /// <returns><see langword="true"/> if the object was succesfully added, otherwise <see langword="false"/>.</returns>
        public static bool TryAddObject(RemoteAdminObject remoteAdminPlayerObject)
        {
            if (remoteAdminPlayerObject is null)
                return false;

            if (string.IsNullOrWhiteSpace(remoteAdminPlayerObject.ListName))
                return false;

            if (_raObjects.Any(obj => remoteAdminPlayerObject.ListName == remoteAdminPlayerObject.ListName))
                return false;

            remoteAdminPlayerObject.AssignedId = _idGenerator.Next();
            remoteAdminPlayerObject.IsActive = true;

            _raObjects.Add(remoteAdminPlayerObject);
            return true;
        }

        /// <summary>
        /// Tries to retrieve a Remote Admin object by it's type.
        /// </summary>
        /// <typeparam name="T">The type to get.</typeparam>
        /// <param name="raObject">The found object instance.</param>
        /// <returns></returns>
        public static bool TryGetObject<T>(out T raObject) where T : RemoteAdminObject
            => _raObjects.TryGetFirst(obj => obj is T, out raObject);

        public static bool TryGetObject(int assignedId, out RemoteAdminObject raObject)
            => _raObjects.TryGetFirst(obj => obj.AssignedId == assignedId, out raObject);

        public static bool TryGetObject(string customId, out RemoteAdminObject raObject)
            => _raObjects.TryGetFirst(obj => !string.IsNullOrWhiteSpace(obj.CustomId) && obj.CustomId == customId, out raObject);

        public static bool TryGetObject<T>(int assignedId, out T raObject) where T : RemoteAdminObject
            => _raObjects.TryGetFirst(obj => obj.AssignedId == assignedId, out raObject);

        public static bool TryGetObject<T>(string customId, out T raObject) where T : RemoteAdminObject
            => _raObjects.TryGetFirst(obj => !string.IsNullOrWhiteSpace(obj.CustomId) && obj.CustomId == customId, out raObject);

        public static RemoteAdminObject GetObject(int assignedId)
            => _raObjects.FirstOrDefault(obj => obj.AssignedId == assignedId);

        public static RemoteAdminObject GetObject(string customId)
            => _raObjects.FirstOrDefault(obj => !string.IsNullOrWhiteSpace(obj.CustomId) && obj.CustomId == customId);

        public static T GetObject<T>() where T : RemoteAdminObject
            => _raObjects.TryGetFirst(obj => obj is T, out var raObject) ? (T)raObject : default;

        public static T GetObject<T>(int assignedId) where T : RemoteAdminObject
            => _raObjects.TryGetFirst(obj => obj is T && obj.AssignedId == assignedId, out var raObject) ? (T)raObject : default;

        public static T GetObject<T>(string customId) where T : RemoteAdminObject
            => _raObjects.TryGetFirst(obj => obj is T && !string.IsNullOrWhiteSpace(obj.CustomId) && obj.CustomId == customId, out var raObject) ? (T)raObject : default;

        public static bool TryRemoveObject(int assignedId)
            => _raObjects.List.RemoveAll(obj => obj.AssignedId == assignedId) > 0;

        public static bool TryRemoveObject(string customId)
            => _raObjects.List.RemoveAll(obj => !string.IsNullOrWhiteSpace(obj.CustomId) && obj.CustomId == customId) > 0;

        public static T[] GetAllObjects<T>() where T : RemoteAdminObject
            => _raObjects.Where(raObj => raObj is T).CastArray<T>();

        public static bool ClearObjects<T>() where T : RemoteAdminObject
        {
            if (_raObjects.List.RemoveAll(raObject => raObject is T) > 0)
            {
                _idGenerator.FreeAll(id => !_raObjects.Any(raObject => raObject.AssignedId == id));
                return true;
            }

            return false;
        }

        internal static void ClearObjects()
        {
            _raObjects.List.RemoveAll(raObject => !raObject.IsActive || !raObject.KeepOnRoundRestart);
            _idGenerator.FreeAll(id => !_raObjects.Any(raObject => raObject.AssignedId == id));
        }

        internal static void ClearWhitelisted(uint leftId)
        {
            foreach (var obj in _raObjects)
            {
                if (obj is WhitelistedRemoteAdminObject whitelistedRemoteAdminObject)
                    whitelistedRemoteAdminObject._visibleTo.Remove(leftId);
            }
        }
    }
}
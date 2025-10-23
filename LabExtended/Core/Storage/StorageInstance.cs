using LabExtended.Utilities;
using LabExtended.Utilities.Update;

using Mirror;

using System.Diagnostics;

namespace LabExtended.Core.Storage
{
    /// <summary>
    /// An active storage instance.
    /// </summary>
    public class StorageInstance
    {
        private NetworkWriter writer = new();
        private NetworkReader reader = new(default);

        private PlayerUpdateComponent updateComponent;

        private Stopwatch writeGuard;
        private Stopwatch updateGuard;

        private FileSystemSafeWatcher watcher;

        internal List<StorageValue> values = new();
        
        private Dictionary<string, StorageValue> lookup = new();

        /// <summary>
        /// Gets the name of this storage instance.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the full path to the folder of this storage instance.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Gets or sets the amount of milliseconds that must pass between writes performed by this server.
        /// </summary>
        public int WriteGuard { get; set; } = 300;

        /// <summary>
        /// Gets or sets the amount of milliseconds that must pass between each update tick.
        /// </summary>
        public int UpdateGuard { get; set; } = 100;

        /// <summary>
        /// Gets or sets how many times a dirty value can be attempted to be written before the change is ignored.
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Gets all values in this storage instance.
        /// </summary>
        public IReadOnlyList<StorageValue> Values => values;

        /// <summary>
        /// Gets a read-only dictionary that maps keys to their corresponding storage values.
        /// </summary>
        public IReadOnlyDictionary<string, StorageValue> Lookup => lookup;

        /// <summary>
        /// Retrieves an existing value of the specified type by name, or adds a new value created by the provided
        /// factory function.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve or add. Must derive from <see cref="StorageValue"/>.</typeparam>
        /// <param name="name">The name of the value to retrieve or add. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="factory">A function that creates a new value of type <typeparamref name="T"/> if the value does not already exist.
        /// Cannot be <see langword="null"/>.</param>
        /// <returns>The existing value associated with the specified name, or the newly created value if no value was found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/> or empty, or if <paramref name="factory"/> is
        /// <see langword="null"/>.</exception>
        public T GetOrAdd<T>(string name, Func<T> factory) where T : StorageValue
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (TryGet<T>(name, out var value))
                return value;

            if (factory is null)
                throw new ArgumentNullException(nameof(factory));

            value = factory();

            if (value is null)
                throw new Exception($"Factory provided a null value");

            if (string.IsNullOrEmpty(value.Name))
                value.Name = name;

            if (!Add(value))
                throw new Exception($"Factory value could not be added");

            return value;
        }

        /// <summary>
        /// Retrieves the value associated with the specified name if it exists; otherwise, creates, adds, and returns a
        /// new value using the provided factory function.
        /// </summary>
        /// <param name="name">The unique name associated with the value to retrieve or add. Cannot be null or empty.</param>
        /// <param name="factory">A function that creates a new <see cref="StorageValue"/> if the specified name does not exist. Cannot be
        /// null.</param>
        /// <returns>The <see cref="StorageValue"/> associated with the specified name. If the name does not exist, the value
        /// created by the <paramref name="factory"/> is added and returned.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null or empty, or if <paramref name="factory"/> is null.</exception>
        public StorageValue GetOrAdd(string name, Func<StorageValue> factory)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (TryGet(name, out var value))
                return value;

            if (factory is null)
                throw new ArgumentNullException(nameof(factory));

            value = factory();

            if (value is null)
                throw new Exception($"Factory provided a null value");

            if (string.IsNullOrEmpty(value.Name))
                value.Name = name;

            if (!Add(value))
                throw new Exception($"Factory value could not be added");

            return value;
        }

        /// <summary>
        /// Retrieves a stored value of the specified type by its name.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve. Must derive from <see cref="StorageValue"/>.</typeparam>
        /// <param name="name">The name of the value to retrieve. Cannot be <see langword="null"/> or empty.</param>
        /// <returns>The stored value of type <typeparamref name="T"/> associated with the specified name.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no value is found for the specified <paramref name="name"/>.</exception>
        public T Get<T>(string name) where T : StorageValue
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (!TryGet<T>(name, out var value))
                throw new KeyNotFoundException($"Value '{name}' could not be found");

            return value;
        }

        /// <summary>
        /// Retrieves the value associated with the specified name.
        /// </summary>
        /// <param name="name">The name of the value to retrieve. Cannot be <see langword="null"/> or empty.</param>
        /// <returns>The <see cref="StorageValue"/> associated with the specified name.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no value is found for the specified <paramref name="name"/>.</exception>
        public StorageValue Get(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (!TryGet(name, out var value))
                throw new KeyNotFoundException($"Value '{name}' could not be found");

            return value;
        }

        /// <summary>
        /// Attempts to retrieve a value of the specified type associated with the given name.
        /// </summary>
        /// <remarks>This method performs a type check to ensure the retrieved value matches the specified
        /// type <typeparamref name="T"/>. If the name does not exist in the lookup or the value cannot be cast to
        /// <typeparamref name="T"/>, the method returns <see langword="false"/>.</remarks>
        /// <typeparam name="T">The type of the value to retrieve. Must derive from <see cref="StorageValue"/>.</typeparam>
        /// <param name="name">The name associated with the value to retrieve.</param>
        /// <param name="value">When this method returns, contains the value of type <typeparamref name="T"/> associated with the specified
        /// name, if the retrieval was successful; otherwise, the default value for the type <typeparamref name="T"/>.</param>
        /// <returns><see langword="true"/> if a value of type <typeparamref name="T"/> was successfully retrieved; otherwise,
        /// <see langword="false"/>.</returns>
        public bool TryGet<T>(string name, out T value) where T : StorageValue
        {
            value = null!;

            if (!lookup.TryGetValue(name, out var storageValue))
                return false;

            if (storageValue is not T castValue)
                return false;

            value = castValue;
            return true;
        }

        /// <summary>
        /// Attempts to retrieve the value associated with the specified name.
        /// </summary>
        /// <param name="name">The key whose associated value is to be retrieved.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key,  if the key is found;
        /// otherwise, the default value for the type of the <see cref="StorageValue"/> parameter. This parameter is
        /// passed uninitialized.</param>
        /// <returns><see langword="true"/> if the key was found and the value was successfully retrieved;  otherwise, <see
        /// langword="false"/>.</returns>
        public bool TryGet(string name, out StorageValue value)
            => lookup.TryGetValue(name, out value);

        /// <summary>
        /// Adds the specified <see cref="StorageValue"/> to the collection if it does not already exist.
        /// </summary>
        /// <remarks>When a <see cref="StorageValue"/> is added, its <see cref="StorageValue.DirtyBit"/>
        /// and <see cref="StorageValue.Path"/>  are initialized, and the <see cref="StorageValue.OnAdded"/> and <see
        /// cref="StorageValue.OnLoaded"/> callbacks are invoked.  The method also ensures the <see
        /// cref="StorageValue"/> is associated with the current storage instance.</remarks>
        /// <param name="value">The <see cref="StorageValue"/> to add. The value must have a non-null, non-whitespace name.</param>
        /// <returns><see langword="true"/> if the <see cref="StorageValue"/> was successfully added;  otherwise, <see
        /// langword="false"/> if a value with the same name already exists in the collection.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="Exception">Thrown if <paramref name="value"/> has a null or whitespace <see cref="StorageValue.Name"/>.</exception>
        public bool Add(StorageValue value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (string.IsNullOrWhiteSpace(value.Name))
                throw new Exception($"Provided storage value does not have a name!");

            if (lookup.ContainsKey(value.Name))
                return false;

            value.Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Path, value.Name));

            value.Storage = this;
            value.OnAdded();

            Internal_ReadFile(value, false);

            value.OnLoaded();

            values.Add(value);
            lookup.Add(value.Name, value);

            return true;
        }

        /// <summary>
        /// Removes the specified <see cref="StorageValue"/> from the collection and optionally deletes its associated
        /// file.
        /// </summary>
        /// <param name="value">The <see cref="StorageValue"/> to remove. Must not be <see langword="null"/> and must have a valid name.</param>
        /// <param name="deleteFile">A value indicating whether to delete the file associated with the <paramref name="value"/>.  If <see
        /// langword="true"/>, the file will be deleted if it exists.</param>
        /// <returns><see langword="true"/> if the <paramref name="value"/> was successfully removed; otherwise, <see
        /// langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="Exception">Thrown if <paramref name="value"/> does not have a valid name.</exception>
        public bool Remove(StorageValue value, bool deleteFile = false)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (string.IsNullOrWhiteSpace(value.Name))
                throw new Exception($"Provided storage value does not have a name!");

            if (!lookup.Remove(value.Name))
                return false;

            if (deleteFile && File.Exists(value.Path))
                File.Delete(value.Path);

            value.OnDestroyed();

            value.Storage = null!;
            value.IsDirty = false;
            value.Path = string.Empty;

            return true;
        }

        /// <summary>
        /// Initializes the necessary components and sets up file watching and update handling.
        /// </summary>
        public virtual void Initialize()
        {
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);

            writeGuard = new();
            updateGuard = new();

            watcher = new(Path) 
            { 
                EnableRaisingEvents = true, 
                IncludeSubdirectories = true,

                ConsolidationInterval = 50, 

                NotifyFilter = NotifyFilters.LastWrite,

                IgnoreChange = Internal_CheckGuard 
            };

            watcher.Changed += Internal_FileChanged;

            updateComponent = PlayerUpdateComponent.Create();
            updateComponent.OnFixedUpdate += Internal_Update;

            updateGuard.Restart();
        }

        /// <summary>
        /// Releases all resources used by the instance and performs necessary cleanup.
        /// </summary>
        public void Destroy()
        {
            if (updateComponent != null)
            {
                updateComponent.OnFixedUpdate -= Internal_Update;

                updateComponent.Destroy();
                updateComponent = null!;
            }

            writer = null!;
            reader = null!;

            writeGuard?.Stop();
            writeGuard = null!;

            watcher?.Dispose();
            watcher = null!;

            values.ForEach(x => x.OnDestroyed());
            values.Clear();

            lookup.Clear();
        }

        /// <summary>
        /// Marks all values as dirty and updates the internal collection of dirty items.
        /// </summary>
        /// <remarks>This method iterates through all values, marking each as dirty, and then updates the 
        /// internal collection to reflect the current state of dirty items. It clears the existing  collection of dirty
        /// items before repopulating it.</remarks>
        public void Save()
        {
            values.ForEach(x => x.IsDirty = true);
        }

        private void Internal_Update()
        {
            if (UpdateGuard > 0 && updateGuard != null)
            {
                if (updateGuard.ElapsedMilliseconds < UpdateGuard)
                    return;

                updateGuard.Restart();
            }

            Internal_WriteDirty();
        }

        private void Internal_ReadFile(StorageValue value, bool isRemote)
        {
            if (File.Exists(value.Path))
            {
                try
                {
                    var bytes = File.ReadAllBytes(value.Path);
                    var segment = new ArraySegment<byte>(bytes);

                    reader.SetBuffer(segment);

                    value.IsDirty = false;
                    value.ReadValue(reader);

                    if (isRemote)
                        value.OnChanged();
                }
                catch (Exception ex)
                {
                    ApiLog.Error("StorageManager", $"Could not apply read value of &3{value.ValuePath}&r due to an exception:\n{ex}");
                }
            }
            else
            {
                value.ApplyDefault();
            }
        }

        private void Internal_WriteDirty()
        {
            if (values.Count > 0)
            {
                var anyNull = false;

                values.ForEach(value =>
                {
                    if (value.Storage is null)
                    {
                        anyNull = true;
                        return;
                    }

                    if (!value.IsDirty)
                        return;                
                    
                    if (value.dirtyRetries > MaxRetries)
                    {
                        value.IsDirty = false;

                        ApiLog.Warn("StorageManager", $"Dirty value of &3{value.ValuePath}&r will be discarded due to exceeding maximum retry count!");
                        return;
                    }

                    writer.Reset();

                    value.WriteValue(writer);

                    if (writer.Position > 0)
                    {
                        try
                        {
                            writeGuard.Restart();

                            using var fileStream = new FileStream(value.Path, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.None);

                            fileStream.Write(writer.buffer, 0, writer.Position);

                            value.IsDirty = false;
                            value.OnSaved();
                        }
                        catch (Exception ex)
                        {
                            ApiLog.Error("StorageManager", $"Could not write the value of &3{value.ValuePath}&r due to an exception:\n{ex}");

                            value.dirtyRetries++;
                        }
                    }
                    else
                    {
                        value.IsDirty = false;

                        ApiLog.Warn("StorageManager", $"Value &3{value.ValuePath}&r did not write any data into the buffer!");
                    }
                });

                if (anyNull)
                    values.RemoveAll(x => x.Storage is null);
            }
        }

        private void Internal_FileChanged(object _, FileSystemEventArgs args)
        {
            var fullPath = System.IO.Path.GetFullPath(args.FullPath);
            var targetValue = values.FirstOrDefault(x => x.Path == fullPath);

            if (targetValue is null)
                return;

            Internal_ReadFile(targetValue, true);
        }

        private bool Internal_CheckGuard(FileSystemEventArgs args)
        {
            var isActive = WriteGuard > 0
                && writeGuard != null
                && writeGuard.IsRunning
                && writeGuard.ElapsedMilliseconds < WriteGuard;

            return isActive;
        }
    }
}
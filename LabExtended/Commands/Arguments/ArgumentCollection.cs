﻿using LabExtended.API.Pooling;

namespace LabExtended.Commands.Arguments
{
    public class ArgumentCollection : IDisposable
    {
        private Dictionary<string, object> _args = DictionaryPool<string, object>.Rent();

        public int Size => _args.Count;

        public bool IsDisposed => _args is null;

        public void Add(string name, object value)
            => _args[name.ToLower()] = value;

        public bool Has(string name)
            => _args.ContainsKey(name.ToLower());

        public bool Has(string name, out object value)
            => _args.TryGetValue(name.ToLower(), out value);

        public bool Has<T>(string name, out T value)
            => (_args.TryGetValue(name.ToLower(), out var result) ? value = (T)result : value = default) != null;

        public T Get<T>(string name)
            => (T)_args[name.ToLower()];

        public void ClearCollection()
            => _args.Clear();

        public void Dispose()
        {
            if (_args is null)
                return;

            DictionaryPool<string, object>.Return(_args);

            _args = null;
        }
    }
}
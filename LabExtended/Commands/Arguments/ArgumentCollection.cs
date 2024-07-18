using LabExtended.API.Collections.Locked;

namespace LabExtended.Commands.Arguments
{
    public class ArgumentCollection
    {
        private readonly LockedDictionary<string, object> _args = new LockedDictionary<string, object>();

        public int Size => _args.Count;

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
    }
}
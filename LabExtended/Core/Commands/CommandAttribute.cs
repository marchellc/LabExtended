namespace LabExtended.Core.Commands
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CommandAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        public string[] Aliases { get; }

        public CommandAttribute(string name, string description, params string[] aliases)
        {
            Name = name;
            Description = description;
            Aliases = aliases;
        }
    }
}
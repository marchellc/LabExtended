using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Attributes
{
    /// <summary>
    /// Specifies a restriction to apply to a command parameter by associating an implementation of the
    /// ICommandParameterRestriction interface with the parameter.
    /// </summary>
    /// <remarks>Apply this attribute to a method parameter to enforce custom validation or restriction logic
    /// when parsing command input. Multiple instances of this attribute can be applied to a single parameter to combine
    /// multiple restrictions. The restriction type must implement the ICommandParameterRestriction interface and be
    /// able to load any required parameters from the provided restriction string.</remarks>
    [AttributeUsage(AttributeTargets.Parameter,     
        Inherited = false,
        AllowMultiple = true)]
    public class CommandParameterAttribute : Attribute
    {
        /// <summary>
        /// Gets the parsed command parameter restriction.
        /// </summary>
        public ICommandParameterRestriction? Restriction { get; private set; }

        #region Custom Parsers
        /// <summary>
        /// Gets or sets the type of parser to use for the command parameter.
        /// </summary>
        public Type? ParserType { get; set; } = null;

        /// <summary>
        /// Gets or sets the name or path to the property in the parser.
        /// </summary>
        public string? ParserProperty { get; set; } = null;
        #endregion

        #region Custom Restrictions
        /// <summary>
        /// Gets or sets the type of restriction to apply to the command parameter.
        /// </summary>
        public Type? RestrictionType { get; set; } = null;

        /// <summary>
        /// Gets or sets the restriction string used to configure the restriction instance.
        /// </summary>
        public string? RestrictionString { get; set; } = null;
        #endregion

        /// <summary>
        /// Gets or sets the name of the command parameter.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the command parameter.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the usage alias of the command parameter.
        /// </summary>
        public string? UsageAlias { get; set; }

        /// <summary>
        /// Initializes a new instance of the CommandParameterOptionsAttribute class.
        /// </summary>
        public CommandParameterAttribute()
        {
            InitRestriction();
        }

        /// <summary>
        /// Initializes a new instance of the CommandParameterAttribute class with the specified parameter name and
        /// description.
        /// </summary>
        /// <param name="name">The name of the command parameter. Cannot be null or empty.</param>
        /// <param name="description">A description of the command parameter. If not specified, defaults to "No description".</param>
        public CommandParameterAttribute(string name, string description = "No description")
        {
            Name = name;
            Description = description;

            InitRestriction();
        }

        /// <summary>
        /// Initializes a new instance of the CommandParameterAttribute class with the specified restriction types. This
        /// constructor is obsolete.
        /// </summary>
        /// <remarks>This constructor is deprecated. Use the parameterless constructor and set the
        /// RestrictionType property separately to specify restrictions. Using this constructor will result in a
        /// compile-time error.</remarks>
        /// <param name="restrictionTypes">An array of types used to restrict the allowed values for the parameter. Only the first type in the array is
        /// used.</param>
        [Obsolete("This constructor is deprecated. Use the constructor without restrictionTypes and set RestrictionType property separately.", true)]
        public CommandParameterAttribute(params Type[] restrictionTypes)
        {
            if (restrictionTypes.Length > 0)
                RestrictionType = restrictionTypes[0];

            InitRestriction();
        }

        /// <summary>
        /// Initializes a new instance of the CommandParameterAttribute class with the specified name, description, and
        /// restriction types.
        /// </summary>
        /// <remarks>This constructor is obsolete. Use the constructor without restrictionTypes and set
        /// the RestrictionType property separately.</remarks>
        /// <param name="name">The name of the command parameter. Cannot be null.</param>
        /// <param name="description">A description of the command parameter. Cannot be null.</param>
        /// <param name="restrictionTypes">An array of types that restrict the allowed values for the parameter. If multiple types are provided, only
        /// the first is used.</param>
        [Obsolete("This constructor is deprecated. Use the constructor without restrictionTypes and set RestrictionType property separately.", true)]
        public CommandParameterAttribute(string name, string description, params Type[] restrictionTypes)
        {
            Name = name;
            Description = description;

            if (restrictionTypes.Length > 0)
                RestrictionType = restrictionTypes[0];

            InitRestriction();
        }

        private void InitRestriction()
        {
            if (RestrictionType != null)
            {
                if (Activator.CreateInstance(RestrictionType) is not ICommandParameterRestriction restrictionInstance)
                    throw new ArgumentException($"Type {RestrictionType.FullName} does not implement ICommandParameterRestriction interface.", 
                        nameof(RestrictionType));

                if (!restrictionInstance.TryLoad(RestrictionString ?? string.Empty))
                    throw new ArgumentException($"Failed to load restriction string '{RestrictionString}' for restriction type {RestrictionType.FullName}.",
                        nameof(RestrictionString));

                Restriction = restrictionInstance;
            }
        }
    }
}
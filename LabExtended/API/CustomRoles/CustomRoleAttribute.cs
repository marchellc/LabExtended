namespace LabExtended.API.CustomRoles;

public class CustomRoleAttribute : Attribute
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public bool AssignOnJoin { get; set; } = false;
    
    public string PredicateName { get; set; } = string.Empty;

    internal Type roleType;
    internal Func<ExPlayer, bool> predicate;
    internal Func<object[], object> constructor;
    internal List<CustomRole> roles;
}
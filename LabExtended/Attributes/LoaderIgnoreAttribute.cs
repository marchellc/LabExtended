namespace LabExtended.Attributes;

/// <summary>
/// Tells the loader and any other auto-registering mechanism to ignore this class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class LoaderIgnoreAttribute : Attribute  { }
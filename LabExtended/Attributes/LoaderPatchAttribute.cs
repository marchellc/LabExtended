namespace LabExtended.Attributes;

/// <summary>
/// Instructs the loader that it should patch all patches inside a plugin.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class LoaderPatchAttribute : Attribute { }
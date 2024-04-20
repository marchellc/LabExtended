using System.Reflection;

using Common.Logging;

namespace LabExtended.Plugins;

public class ExPlugin
{
	public ExConfig Config { get; internal set; }
	public LogOutput Log { get; internal set; }
	
	public Assembly Assembly { get; }

	public ExPlugin()
	{
		var asm = Assembly.GetExecutingAssembly();
		
		var name = asm.GetName();
		
		var desc = asm.GetCustomAttribute<AssemblyDescriptionAttribute>();
		var comp = asm.GetCustomAttribute<AssemblyCompanyAttribute>();
		
		Name = name.Name;
		
		Author = comp?.Company ?? "Unknown";
		Description = desc?.Description ?? "None";

		Version = name.Version ?? new Version(1, 0, 0);

		Assembly = asm;
	}
	
	public virtual string Name { get; } 
	public virtual string Author { get; }
	public virtual string Description { get; }
	
	public virtual Version Version { get; }
	
	public virtual VersionRange? ApiCompatibility { get; }
	public virtual VersionRange? GameCompatibility { get; }
	
	public virtual void OnLoaded() { }
	public virtual void OnUnloaded() { }
	public virtual void OnReloaded() { }
}
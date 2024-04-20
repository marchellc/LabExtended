using System.Reflection;

using Configuration;
using Configuration.Parsers;

namespace LabExtended.Plugins;

public class ExConfig
{
	private readonly string _path;
	private readonly Assembly _asm;

	public ExConfig(string path, Assembly asm)
	{
		_path = path;
		_asm = asm;
	}
    
	public ConfigFile Config { get; private set; }

	public bool IsWatched
	{
		get => Config.Watcher != null && Config.Watcher.IsEnabled;
		set => Config.Watcher!.IsEnabled = value;
	}

	public void Load()
	{
		if (Config is null)
			SetupConfig();
		
		Config.Load();
	}

	public void Unload()
	{
		if (Config is null)
			return;
		
		Config.Save();
		Config.UnbindAll();
		Config = null;
	}

	private void SetupConfig()
	{
		Config = new ConfigFile(_path, true, YamlParser.Parser);
		Config.Bind(_asm);
	}
}
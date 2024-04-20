using System.Reflection;

using Common.Extensions;
using Common.IO.Collections;
using Common.Logging;

using LabExtended.Core.Logging;
using LabExtended.Extensions;
using LabExtended.Plugins;

using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Loader;

namespace LabExtended.Core;

public class ExLoader
{
	private static readonly LockedDictionary<Assembly, List<ExPlugin>> _plugins = new LockedDictionary<Assembly, List<ExPlugin>>();
    
	public static Version GameVersion { get; } = new Version(GameCore.Version.Major, GameCore.Version.Minor, GameCore.Version.Revision);
	public static Version ApiVersion { get; } = new Version(1, 0, 0);
    
	public static ExLoader Loader { get; private set; }

	public static string Folder
	{
		get => Loader.Handler.PluginDirectoryPath;
	}

	public static string Configs
	{
		get => $"{Folder}/Configs";
	}

	public static string Plugins
	{
		get => $"{Folder}/Plugins";
	}

	[PluginConfig] 
	public ExLoaderConfig Config;
	public PluginHandler Handler;

	public VersionRange? GameCompatibility = null;

	[PluginEntryPoint("LabExtended", "1.0.0", "An extension to NW's Plugin API.", "marchellc")]
	public void Load()
	{
		try
		{
			var time = DateTime.Now;
			
			Handler = PluginHandler.Get(this);
			Loader = this;

			Info("Extended Loader", $"Loading version &2{ApiVersion}&r ..");

			if (GameCompatibility.HasValue && !GameCompatibility.Value.InRange(GameVersion))
			{
				Error("Extended Loader", $"Attempted to load for an unsupported game version (&1{GameVersion}&r) - supported: &2{GameCompatibility.Value}&r");
				return;
			}

			if (Config is null)
			{
				Error("Extended Loader", "The plugin's config is missing! Seems like you made an error. Delete it and restart the server.");
				return;
			}
			
			if (Config.Logging.PipeEnabled)
				LogOutput.AddToAll<ExLogger>();

			if (!Directory.Exists(Folder))
			{
				Directory.CreateDirectory(Folder);
				Warn("Extended Loader", "Created the default plugin directory - the PluginAPI should have done that.");
			}

			if (!Directory.Exists(Configs))
			{
				Directory.CreateDirectory(Configs);
				Info("Extended Loader", $"Created the config directory.");
			}

			if (!Directory.Exists(Plugins))
			{
				Directory.CreateDirectory(Plugins);
				Info("Extended Loader", $"Created the plugin directory.");
			}

			foreach (var file in Directory.GetFiles(Plugins, "*.dll", SearchOption.TopDirectoryOnly))
			{
				Debug("Extended Loader", $"Found assembly file: {Path.GetFileName(file)}");

				if (!AssemblyLoader.TryGetAssembly(file, out var asm))
					continue;

				AssemblyLoader.ResolveAssemblyEmbeddedResources(asm);

				var loadedAssemblies = AppDomain.CurrentDomain
					.GetAssemblies()
					.Select(x => $"{x.GetName().Name}&r v&6{x.GetName().Version.ToString(3)}");

				var missingDependencies = asm
					.GetReferencedAssemblies()
					.Select(x => $"{x.Name}&r v&6{x.Version.ToString(3)}")
					.Where(x => !loadedAssemblies.Contains(x));

				Type[] types = null;

				try
				{
					types = asm.GetTypes();
				}
				catch (Exception ex)
				{
					if (missingDependencies.Count() != 0)
					{
						Error("Extended Loader", $"Failed loading plugin &2{Path.GetFileNameWithoutExtension(file)}&r, missing dependencies!\n&2{string.Join("\n", missingDependencies.Select(x => $"&r - &2{x}&r"))}\n\n{ex.ToColoredString()}");
						continue;
					}

					Error("Extended Loader", $"Failed loading plugin &2{Path.GetFileNameWithoutExtension(file)}&r!\n{ex.ToColoredString()}");
					continue;
				}

				for (int i = 0; i < types.Length; i++)
				{
					var type = types[i];

					try
					{
						if (!type.InheritsType<ExPlugin>())
							continue;

						var plugin = type.Construct<ExPlugin>();

						if (plugin is null)
						{
							Error("Extended Loader", $"Failed to instantiate type &2{type.FullName}&r!");
							continue;
						}

						if (!_plugins.ContainsKey(asm))
							_plugins[asm] = new List<ExPlugin>() { plugin };
						else
							_plugins[asm].Add(plugin);

						Info("Extended Loader", $"Found plugin &2{plugin.Name}&r by &3{plugin.Author}&r! (&2{plugin.Version}&r)");
					}
					catch (Exception ex)
					{
						Error("Extended Loader", $"Failed loading type &2{type.FullName}&r!\n{ex.ToColoredString()}");
					}
				}

				if (_plugins.Count < 1)
				{
					Info("Extended Loader", "No plugins were found.");
					return;
				}

				var failed = new List<ExPlugin>();

				foreach (var pair in _plugins)
				{
					foreach (var plugin in pair.Value)
					{
						if (plugin.ApiCompatibility.HasValue && !plugin.ApiCompatibility.Value.InRange(ApiVersion))
						{
							failed.Add(plugin);

							Warn("Extended Loader", $"Plugin &2{plugin.Name}&r is not compatible with this API version (&2{ApiVersion}&r)! - compatibility: &3{plugin.ApiCompatibility.Value}&r");
							continue;
						}

						if (plugin.GameCompatibility.HasValue && !plugin.GameCompatibility.Value.InRange(GameVersion))
						{
							failed.Add(plugin);

							Warn("Extended Loader", $"Plugin &2{plugin.Name}&r is not compatible with this game version (&2{GameVersion}&r)! - compatibility: &3{plugin.GameCompatibility.Value}&r");
							continue;
						}

						Debug("Extended Loader", $"Loading plugin {plugin.Name} ..");

						try
						{
							plugin.Config = new ExConfig($"{Configs}/{plugin.Name}", pair.Key);
							plugin.Config.Load();

							plugin.Log = new LogOutput() { Name = plugin.Name };

							if (CanDebug(plugin.Name))
								plugin.Log.Enabled |= LogUtils.Debug;

							plugin.OnLoaded();
						}
						catch (Exception ex)
						{
							Error("Extended Loader", $"Failed while loading plugin &2{plugin.Name}&r:\n{ex.ToColoredString()}");
						}
					}
				}

				foreach (var plugin in failed)
				{
					foreach (var pair in _plugins)
					{
						pair.Value.Remove(plugin);
					}
				}

				var end = (DateTime.Now - time).TotalMilliseconds;

				if (failed.Count > 0)
					Warn("Extended Loader", $"Finished loading with {failed.Count} error(s), took {end} ms.");
				else
					Info("Extended Loader", $"Finished loading {_plugins.Sum(p => p.Value.Count)} plugin(s), took {end} ms.");
			}
		}
		catch (Exception ex)
		{
			Error("Extended Loader", $"A general loading error has occured!\n{ex.ToColoredString()}");
		}
	}

	public static void Info(string source, object message)
	{
		if (message is Exception ex)
			message = ex.ToColoredString();
		
		Log.Info(message.ToString(), source);
	}

	public static void Warn(string source, object message)
	{
		if (message is Exception ex)
			message = ex.ToColoredString();
		
		Log.Warning(message.ToString(), source);
	}

	public static void Error(string source, object message)
	{
		if (message is Exception ex)
			message = ex.ToColoredString();
		
		Log.Error(message.ToString(), source);
	}

	public static void Debug(string source, object message)
	{
		if (!CanDebug(source))
			return;
		
		if (message is Exception ex)
			message = ex.ToColoredString();
		
		Log.Debug(message.ToString(), source);
	}

	private static bool CanDebug(string source)
	{
		if (!string.IsNullOrWhiteSpace(source))
		{
			if (Loader.Config.Logging.DisabledSources.Contains(source))
				return false;

			if (Loader.Config.Logging.EnabledSources.Contains(source))
				return true;
		}

		return Loader.Config.Logging.DebugEnabled;
	}
}
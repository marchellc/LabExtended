using System.ComponentModel;

using LabExtended.Core.Configs;

namespace LabExtended.Core;

public class ExLoaderConfig
{
	[Description("Logging configuration.")]
	public LogConfig Logging { get; set; } = new LogConfig();
}
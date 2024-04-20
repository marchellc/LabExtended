using System.ComponentModel;

namespace LabExtended.Core.Configs;

public class HookConfig
{
	[Description("Toggles global event time recording.")]
	public bool TimeRecording { get; set; } = true;

	[Description("Sets a list of events that record time.")]
	public List<string> CustomRecording { get; set; } = new List<string>();
	
	[Description("Sets the amount of milliseconds to wait for an async event to finish (defaults to 1 000 ms).")]
	public float AsyncTimeout { get; set; } = 1000f;

	[Description("Sets a list of custom async timeouts. Keys need to be formatted as 'Namespace.TypeName' for events and 'Namespace.TypeName.MethodName' for hooks.")]
	public Dictionary<string, float> CustomTimeouts { get; set; } = new Dictionary<string, float>();
}
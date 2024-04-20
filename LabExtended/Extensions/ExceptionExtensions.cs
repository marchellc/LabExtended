namespace LabExtended.Extensions;

public static class ExceptionExtensions
{
	public static string ToColoredString(this Exception ex)
		=> ex.ToString()
			.Replace("at", "&3at&r")
			.Replace("(", "&2(&r")
			.Replace(")", "&2)&r")
			.Replace(".", "&1.&r");
}
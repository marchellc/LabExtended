namespace LabExtended.Plugins;

public struct VersionRange
{
	public readonly Version Minimal;
	public readonly Version Maximal;
	public readonly Version Specific;

	public VersionRange(Version minimal, Version maximal)
	{
		Minimal = minimal;
		Maximal = maximal;
	}

	public VersionRange(Version specific)
	{
		Specific = specific;
	}

	public bool InRange(Version version)
	{
		if (Specific != null && (version.Major != Specific.Major || version.Minor != Specific.Minor || version.Build != Specific.Build))
			return false;
		
		if (Minimal != null && (version.Major < Minimal.Major || version.Minor < Minimal.Minor || version.Build < Minimal.Build))
			return false;

		if (Maximal != null && (version.Major > Maximal.Major || version.Minor > Maximal.Minor || version.Build > Maximal.Build))
			return false;

		return true;
	}

	public override string ToString()
	{
		var str = "Version Range (";

		if (Minimal != null)
			str += $"Min={Minimal} ";

		if (Maximal != null)
			str += $"Max={Maximal} ";

		if (Specific != null)
			str += $"{Specific}";

		return (str + ")").Trim();
	}
}
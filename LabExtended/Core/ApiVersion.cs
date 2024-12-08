using LabExtended.Utilities;

namespace LabExtended.Core
{
    public static class ApiVersion
    {
        public const int Major = 1;
        public const int Minor = 0;
        public const int Build = 0;
        public const int Patch = 0;

        public static Version Version { get; } = new Version(Major, Minor, Build, Patch);
        public static Version Game { get; } = new Version(GameCore.Version.Major, GameCore.Version.Minor, GameCore.Version.Revision);

        public static VersionRange? Compatibility { get; } = new VersionRange(new Version(14, 0, 0));

        public static bool CheckCompatibility()
        {
            if (ApiLoader.BaseConfig.SkipGameCompatibility)
                return true;

            if (Compatibility.HasValue && !Compatibility.Value.InRange(Game))
            {
                ApiLog.Error("Extended Loader", $"Attempted to load for an unsupported game version (&1{Game}&r) - supported: &2{Compatibility.Value}&r");
                return false;
            }

            return true;
        }
    }
}
using System.Reflection;

namespace Bare3MF.Services;

public static class AppVersion
{
    public static Version Current { get; } = ReadVersion();

    public static string Display => Current.ToString(3);

    private static Version ReadVersion()
    {
        var assembly = typeof(AppVersion).Assembly;
        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informational))
        {
            var versionPart = informational.Split('+', 2)[0];
            if (Version.TryParse(versionPart, out var parsed))
                return Normalize(parsed);
        }

        return assembly.GetName().Version is { } assemblyVersion
            ? Normalize(assemblyVersion)
            : new(1, 0, 0);
    }

    private static Version Normalize(Version version) =>
        new(version.Major, version.Minor, version.Build >= 0 ? version.Build : 0);
}

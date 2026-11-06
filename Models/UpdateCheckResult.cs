namespace Bare3MF.Models;

public sealed record UpdateCheckResult(
    Version CurrentVersion,
    Version LatestVersion,
    string LatestTag,
    string ReleaseUrl,
    string ReleaseName)
{
    public bool IsUpdateAvailable => LatestVersion > CurrentVersion;
}

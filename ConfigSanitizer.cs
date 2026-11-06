using System.IO.Compression;

namespace Bare3MF;

public static class ConfigSanitizer
{
    private const string MetadataPrefix = "Metadata/";
    private const string MetadataPrefixAlt = "Metadata\\";
    private const string ConfigExtension = ".config";
    private const string ModelConfigSuffix = "_model.config";
    private const string ModelSettingsConfigSuffix = "model_settings.config";

    public static int SanitizeFile(string sourcePath, string destinationPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);

        var overwriteSource = string.Equals(
            Path.GetFullPath(sourcePath),
            Path.GetFullPath(destinationPath),
            StringComparison.OrdinalIgnoreCase);

        var tempPath = overwriteSource
            ? Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.3mf")
            : destinationPath;

        var destinationDirectory = Path.GetDirectoryName(tempPath);
        if (!string.IsNullOrEmpty(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        try
        {
            var removedCount = CopyWithoutProfiles(sourcePath, tempPath);

            if (overwriteSource)
            {
                File.Move(tempPath, destinationPath, overwrite: true);
            }

            return removedCount;
        }
        finally
        {
            if (overwriteSource && File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    private static int CopyWithoutProfiles(string sourcePath, string destinationPath)
    {
        using var sourceStream = File.OpenRead(sourcePath);
        using var sourceArchive = new ZipArchive(sourceStream, ZipArchiveMode.Read);
        using var destinationStream = File.Create(destinationPath);
        using var destinationArchive = new ZipArchive(destinationStream, ZipArchiveMode.Create);

        var entriesToRemove = sourceArchive.Entries
            .Where(entry => ShouldRemove(entry.FullName))
            .Select(entry => entry.FullName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in sourceArchive.Entries)
        {
            if (!entriesToRemove.Contains(entry.FullName))
            {
                CopyEntry(entry, destinationArchive);
            }
        }

        return entriesToRemove.Count;
    }

    private static void CopyEntry(ZipArchiveEntry sourceEntry, ZipArchive destinationArchive)
    {
        var newEntry = destinationArchive.CreateEntry(sourceEntry.FullName, CompressionLevel.Optimal);
        var lastWriteTime = sourceEntry.LastWriteTime;

        if (lastWriteTime.Year is >= 1980 and <= 2107)
        {
            newEntry.LastWriteTime = lastWriteTime;
        }

        using var source = sourceEntry.Open();
        using var destination = newEntry.Open();
        source.CopyTo(destination);
    }

    private static bool ShouldRemove(string fullName)
    {
        var name = fullName.AsSpan();

        if (!name.EndsWith(ConfigExtension, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!name.StartsWith(MetadataPrefix, StringComparison.OrdinalIgnoreCase)
            && !name.StartsWith(MetadataPrefixAlt, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !name.EndsWith(ModelConfigSuffix, StringComparison.OrdinalIgnoreCase)
            && !name.EndsWith(ModelSettingsConfigSuffix, StringComparison.OrdinalIgnoreCase);
    }
}

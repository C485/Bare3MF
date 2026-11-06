using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bare3MF.Models;

namespace Bare3MF.Services;

public static class UpdateService
{
    private const string LatestReleaseApi =
        "https://api.github.com/repos/C485/Bare3MF/releases/latest";

    private static readonly HttpClient HttpClient = CreateHttpClient();

    public static async Task<UpdateCheckResult?> CheckAsync(
        Version currentVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(currentVersion);

        try
        {
            using var response = await HttpClient
                .GetAsync(LatestReleaseApi, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return null;

            var release = await response.Content
                .ReadFromJsonAsync<GitHubRelease>(cancellationToken)
                .ConfigureAwait(false);

            if (release is null ||
                string.IsNullOrWhiteSpace(release.TagName) ||
                string.IsNullOrWhiteSpace(release.HtmlUrl))
                return null;

            var latestVersion = ParseVersion(release.TagName);
            if (latestVersion is null)
                return null;

            if (!Uri.TryCreate(release.HtmlUrl, UriKind.Absolute, out var releaseUri) ||
                releaseUri.Scheme != Uri.UriSchemeHttps ||
                !releaseUri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
                return null;

            var tag = release.TagName.Trim();
            return new UpdateCheckResult(
                currentVersion,
                latestVersion,
                tag,
                releaseUri.ToString(),
                string.IsNullOrWhiteSpace(release.Name) ? tag : release.Name);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }

    private static Version? ParseVersion(string tag)
    {
        var value = tag.Trim().TrimStart('v', 'V');
        var metadataIndex = value.IndexOf('+');
        if (metadataIndex >= 0)
            value = value[..metadataIndex];

        var prereleaseIndex = value.IndexOf('-');
        if (prereleaseIndex >= 0)
            value = value[..prereleaseIndex];

        if (!Version.TryParse(value, out var version))
            return null;

        return new(
            version.Major,
            version.Minor,
            version.Build >= 0 ? version.Build : 0);
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Bare3MF-UpdateCheck/1.0 (+https://github.com/C485/Bare3MF)");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
        return client;
    }

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; init; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }
}

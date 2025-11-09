using System.Diagnostics;
using System.Text.Json;
using HealthChecks.UI.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthChecks.UI.Core;

public class ApplicationHealthAggregator : IApplicationHealthAggregator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Settings _settings;
    private readonly ILogger<ApplicationHealthAggregator> _logger;
    private const int DefaultTimeoutSeconds = 3;

    public ApplicationHealthAggregator(
        IHttpClientFactory httpClientFactory,
        IOptions<Settings> settings,
        ILogger<ApplicationHealthAggregator> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<ApplicationHealthReport>> GetAllApplicationsHealthAsync(CancellationToken cancellationToken = default)
    {
        var reports = new List<ApplicationHealthReport>();

        foreach (var app in _settings.Applications)
        {
            var report = await GetApplicationHealthAsync(app.Key, cancellationToken).ConfigureAwait(false);
            if (report != null)
            {
                reports.Add(report);
            }
        }

        return reports;
    }

    public async Task<ApplicationHealthReport?> GetApplicationHealthAsync(string applicationName, CancellationToken cancellationToken = default)
    {
        if (!_settings.Applications.TryGetValue(applicationName, out var appConfig))
        {
            return null;
        }

        var checkedAt = DateTime.UtcNow;
        var memberTasks = new List<Task<MemberHealthReport>>();

        foreach (var memberName in appConfig.Members)
        {
            var healthCheckSetting = _settings.HealthChecks.FirstOrDefault(h => h.Name == memberName);
            if (healthCheckSetting == null)
            {
                _logger.LogWarning("Health check setting not found for member {MemberName} in application {ApplicationName}", memberName, applicationName);
                continue;
            }

            memberTasks.Add(CheckMemberHealthAsync(memberName, healthCheckSetting.Uri, cancellationToken));
        }

        var memberReports = await Task.WhenAll(memberTasks).ConfigureAwait(false);
        var aggregatedStatus = AggregateStatus(memberReports);
        var healthyCount = memberReports.Count(m => m.Status.Equals("Healthy", StringComparison.OrdinalIgnoreCase));
        var avgDuration = memberReports.Length > 0 ? memberReports.Average(m => m.DurationMs) : 0;

        return new ApplicationHealthReport
        {
            Name = applicationName,
            Status = aggregatedStatus,
            HealthyCount = healthyCount,
            TotalCount = memberReports.Length,
            AverageDurationMs = Math.Round(avgDuration, 2),
            CheckedAt = checkedAt,
            Members = memberReports.ToList()
        };
    }

    private async Task<MemberHealthReport> CheckMemberHealthAsync(string name, string uri, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var report = new MemberHealthReport
        {
            Name = name,
            Uri = uri,
            Status = "Unknown",
            DurationMs = 0,
            Payload = null
        };

        try
        {
            var httpClient = _httpClientFactory.CreateClient(Keys.HEALTH_CHECK_HTTP_CLIENT_NAME);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(DefaultTimeoutSeconds));

            var response = await httpClient.GetAsync(uri, cts.Token).ConfigureAwait(false);
            stopwatch.Stop();
            report.DurationMs = stopwatch.ElapsedMilliseconds;

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            report.Payload = content;

            // Try to parse JSON and extract status
            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    var jsonDoc = JsonDocument.Parse(content);
                    if (jsonDoc.RootElement.TryGetProperty("status", out var statusElement))
                    {
                        report.Status = statusElement.GetString() ?? "Unknown";
                    }
                    else if (jsonDoc.RootElement.TryGetProperty("Status", out var statusElementCapital))
                    {
                        report.Status = statusElementCapital.GetString() ?? "Unknown";
                    }
                    else if (response.IsSuccessStatusCode)
                    {
                        // No explicit status field, but successful response
                        report.Status = "Healthy";
                    }
                    else
                    {
                        report.Status = "Unhealthy";
                    }
                }
                catch (JsonException)
                {
                    // Not JSON, fallback to HTTP status code
                    report.Status = response.IsSuccessStatusCode ? "Healthy" : "Unhealthy";
                }
            }
            else
            {
                // Empty response body
                report.Status = response.IsSuccessStatusCode ? "Healthy" : "Unhealthy";
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Cancellation requested by caller
            stopwatch.Stop();
            report.DurationMs = stopwatch.ElapsedMilliseconds;
            report.Status = "Unreachable";
            report.Payload = "Request cancelled";
        }
        catch (OperationCanceledException)
        {
            // Timeout
            stopwatch.Stop();
            report.DurationMs = stopwatch.ElapsedMilliseconds;
            report.Status = "Unreachable";
            report.Payload = "Timeout after 3 seconds";
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            report.DurationMs = stopwatch.ElapsedMilliseconds;
            report.Status = "Unreachable";
            report.Payload = ex.Message;
            _logger.LogError(ex, "Error checking health for member {MemberName} at {Uri}", name, uri);
        }

        return report;
    }

    private string AggregateStatus(MemberHealthReport[] members)
    {
        if (members.Length == 0)
        {
            return "Unknown";
        }

        // If any member is Unhealthy or Unreachable -> Unhealthy
        if (members.Any(m => m.Status.Equals("Unhealthy", StringComparison.OrdinalIgnoreCase) ||
                             m.Status.Equals("Unreachable", StringComparison.OrdinalIgnoreCase)))
        {
            return "Unhealthy";
        }

        // If any member is Degraded -> Degraded
        if (members.Any(m => m.Status.Equals("Degraded", StringComparison.OrdinalIgnoreCase)))
        {
            return "Degraded";
        }

        // If at least one member is Healthy -> Healthy
        if (members.Any(m => m.Status.Equals("Healthy", StringComparison.OrdinalIgnoreCase)))
        {
            return "Healthy";
        }

        // Otherwise -> Unknown
        return "Unknown";
    }
}

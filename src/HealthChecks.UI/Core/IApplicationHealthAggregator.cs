using HealthChecks.UI.Configuration;

namespace HealthChecks.UI.Core;

public interface IApplicationHealthAggregator
{
    Task<List<ApplicationHealthReport>> GetAllApplicationsHealthAsync(CancellationToken cancellationToken = default);
    Task<ApplicationHealthReport?> GetApplicationHealthAsync(string applicationName, CancellationToken cancellationToken = default);
}

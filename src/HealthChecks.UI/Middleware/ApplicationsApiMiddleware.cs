using System.Text.Json;
using System.Text.Json.Serialization;
using HealthChecks.UI.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.UI.Middleware;

internal class ApplicationsApiMiddleware
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public ApplicationsApiMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
    {
        _ = next;
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: true)
            }
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var aggregator = scope.ServiceProvider.GetRequiredService<IApplicationHealthAggregator>();

        var pathSegments = context.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        
        // Check if there's an application name in the path
        // Path format: /api/health/applications or /api/health/applications/{name}
        var applicationsIndex = Array.FindIndex(pathSegments, s => s.Equals("applications", StringComparison.OrdinalIgnoreCase));
        
        if (applicationsIndex >= 0 && applicationsIndex < pathSegments.Length - 1)
        {
            // Specific application requested
            var applicationName = pathSegments[applicationsIndex + 1];
            var report = await aggregator.GetApplicationHealthAsync(applicationName, context.RequestAborted).ConfigureAwait(false);
            
            if (report == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new { message = $"Application '{applicationName}' not found" }, _jsonSerializerOptions).ConfigureAwait(false);
                return;
            }
            
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsJsonAsync(report, _jsonSerializerOptions).ConfigureAwait(false);
        }
        else
        {
            // All applications requested
            var reports = await aggregator.GetAllApplicationsHealthAsync(context.RequestAborted).ConfigureAwait(false);
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsJsonAsync(reports, _jsonSerializerOptions).ConfigureAwait(false);
        }
    }
}

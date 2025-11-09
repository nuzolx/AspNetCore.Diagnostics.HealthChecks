using System.Net;
using System.Text;
using System.Text.Json;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HealthChecks.UI.Tests;

public class applications_api_should
{
    [Fact]
    public async Task return_all_applications_when_calling_applications_endpoint()
    {
        // Arrange
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddRouting()
                    .AddHealthChecks()
                    .Services
                    .AddHealthChecksUI(setup =>
                    {
                        setup.AddHealthCheckEndpoint("Service1", "http://localhost/health1");
                        setup.AddHealthCheckEndpoint("Service2", "http://localhost/health2");
                    })
                    .AddInMemoryStorage(databaseName: "ApplicationsApiTests");
            })
            .UseConfiguration(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["HealthChecksUI:Applications:TestApp:Members:0"] = "Service1",
                    ["HealthChecksUI:Applications:TestApp:Members:1"] = "Service2"
                })
                .Build())
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(setup =>
                {
                    setup.MapHealthChecks("/health1", new HealthCheckOptions
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                    setup.MapHealthChecks("/health2", new HealthCheckOptions
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                    setup.MapHealthChecksUI();
                });
            });

        using var server = new TestServer(webHostBuilder);

        // Act
        var response = await server.CreateRequest("/api/health/applications").GetAsync();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var applications = JsonSerializer.Deserialize<List<ApplicationHealthReport>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        applications.ShouldNotBeNull();
        applications.ShouldNotBeEmpty();
        applications.ShouldContain(app => app.Name == "TestApp");
    }

    [Fact]
    public async Task return_specific_application_when_calling_application_by_name()
    {
        // Arrange
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddRouting()
                    .AddHealthChecks()
                    .Services
                    .AddHealthChecksUI(setup =>
                    {
                        setup.AddHealthCheckEndpoint("Service1", "http://localhost/health1");
                    })
                    .AddInMemoryStorage(databaseName: "ApplicationsApiTests");
            })
            .UseConfiguration(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["HealthChecksUI:Applications:TestApp:Members:0"] = "Service1"
                })
                .Build())
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(setup =>
                {
                    setup.MapHealthChecks("/health1", new HealthCheckOptions
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                    setup.MapHealthChecksUI();
                });
            });

        using var server = new TestServer(webHostBuilder);

        // Act
        var response = await server.CreateRequest("/api/health/applications/TestApp").GetAsync();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var application = JsonSerializer.Deserialize<ApplicationHealthReport>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        application.ShouldNotBeNull();
        application.Name.ShouldBe("TestApp");
        application.Members.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task return_404_when_application_not_found()
    {
        // Arrange
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddRouting()
                    .AddHealthChecks()
                    .Services
                    .AddHealthChecksUI(setup =>
                    {
                        setup.AddHealthCheckEndpoint("Service1", "http://localhost/health1");
                    })
                    .AddInMemoryStorage(databaseName: "ApplicationsApiTests");
            })
            .UseConfiguration(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["HealthChecksUI:Applications:TestApp:Members:0"] = "Service1"
                })
                .Build())
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(setup =>
                {
                    setup.MapHealthChecks("/health1", new HealthCheckOptions
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                    setup.MapHealthChecksUI();
                });
            });

        using var server = new TestServer(webHostBuilder);

        // Act
        var response = await server.CreateRequest("/api/health/applications/NonExistent").GetAsync();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task not_register_applications_endpoints_when_no_applications_configured()
    {
        // Arrange
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddRouting()
                    .AddHealthChecks()
                    .Services
                    .AddHealthChecksUI(setup =>
                    {
                        setup.AddHealthCheckEndpoint("Service1", "http://localhost/health1");
                    })
                    .AddInMemoryStorage(databaseName: "ApplicationsApiTests");
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(setup =>
                {
                    setup.MapHealthChecks("/health1", new HealthCheckOptions
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                    setup.MapHealthChecksUI();
                });
            });

        using var server = new TestServer(webHostBuilder);

        // Act
        var response = await server.CreateRequest("/api/health/applications").GetAsync();

        // Assert
        // The endpoint should not be found when Applications are not configured
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task return_application_with_aggregated_status()
    {
        // Arrange
        var webHostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .AddRouting()
                    .AddHealthChecks()
                    .Services
                    .AddHealthChecksUI(setup =>
                    {
                        setup.AddHealthCheckEndpoint("Service1", "http://localhost/health1");
                        setup.AddHealthCheckEndpoint("Service2", "http://localhost/health2");
                    })
                    .AddInMemoryStorage(databaseName: "ApplicationsApiTests");
            })
            .UseConfiguration(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["HealthChecksUI:Applications:MyApp:Members:0"] = "Service1",
                    ["HealthChecksUI:Applications:MyApp:Members:1"] = "Service2"
                })
                .Build())
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(setup =>
                {
                    setup.MapHealthChecks("/health1", new HealthCheckOptions
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                    setup.MapHealthChecks("/health2", new HealthCheckOptions
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
                    setup.MapHealthChecksUI();
                });
            });

        using var server = new TestServer(webHostBuilder);

        // Act
        var response = await server.CreateRequest("/api/health/applications/MyApp").GetAsync();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var application = JsonSerializer.Deserialize<ApplicationHealthReport>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        application.ShouldNotBeNull();
        application.Name.ShouldBe("MyApp");
        application.TotalCount.ShouldBe(2);
        application.Members.Count.ShouldBe(2);
        application.Status.ShouldNotBeNull();
        application.CheckedAt.ShouldNotBe(default(DateTime));
    }
}

using System.Net;
using System.Text;
using System.Text.Json;
using HealthChecks.UI.Configuration;
using HealthChecks.UI.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace HealthChecks.UI.Tests;

public class application_health_aggregator_should
{
    [Fact]
    public async Task aggregate_status_as_unhealthy_when_any_member_is_unhealthy()
    {
        // Arrange
        var settings = new Settings();
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service1", Uri = "http://service1/health" });
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service2", Uri = "http://service2/health" });
        settings.Applications.Add("TestApp", new ApplicationConfiguration
        {
            Members = new List<string> { "Service1", "Service2" }
        });

        var httpMessageHandler = new TestHttpMessageHandler((request) =>
        {
            if (request.RequestUri!.ToString().Contains("service1"))
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"status\":\"Healthy\"}", Encoding.UTF8, "application/json")
                };
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable,
                Content = new StringContent("{\"status\":\"Unhealthy\"}", Encoding.UTF8, "application/json")
            };
        });

        var httpClientFactory = CreateHttpClientFactory(httpMessageHandler);
        var aggregator = new ApplicationHealthAggregator(
            httpClientFactory,
            MsOptions.Create(settings),
            Substitute.For<ILogger<ApplicationHealthAggregator>>());

        // Act
        var report = await aggregator.GetApplicationHealthAsync("TestApp");

        // Assert
        report.ShouldNotBeNull();
        report.Status.ShouldBe("Unhealthy");
        report.TotalCount.ShouldBe(2);
        report.HealthyCount.ShouldBe(1);
    }

    [Fact]
    public async Task aggregate_status_as_degraded_when_any_member_is_degraded_and_none_unhealthy()
    {
        // Arrange
        var settings = new Settings();
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service1", Uri = "http://service1/health" });
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service2", Uri = "http://service2/health" });
        settings.Applications.Add("TestApp", new ApplicationConfiguration
        {
            Members = new List<string> { "Service1", "Service2" }
        });

        var httpMessageHandler = new TestHttpMessageHandler((request) =>
        {
            if (request.RequestUri!.ToString().Contains("service1"))
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"status\":\"Healthy\"}", Encoding.UTF8, "application/json")
                };
            }
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"status\":\"Degraded\"}", Encoding.UTF8, "application/json")
            };
        });

        var httpClientFactory = CreateHttpClientFactory(httpMessageHandler);
        var aggregator = new ApplicationHealthAggregator(
            httpClientFactory,
            MsOptions.Create(settings),
            Substitute.For<ILogger<ApplicationHealthAggregator>>());

        // Act
        var report = await aggregator.GetApplicationHealthAsync("TestApp");

        // Assert
        report.ShouldNotBeNull();
        report.Status.ShouldBe("Degraded");
        report.TotalCount.ShouldBe(2);
        report.HealthyCount.ShouldBe(1);
    }

    [Fact]
    public async Task aggregate_status_as_healthy_when_all_members_are_healthy()
    {
        // Arrange
        var settings = new Settings();
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service1", Uri = "http://service1/health" });
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service2", Uri = "http://service2/health" });
        settings.Applications.Add("TestApp", new ApplicationConfiguration
        {
            Members = new List<string> { "Service1", "Service2" }
        });

        var httpMessageHandler = new TestHttpMessageHandler((request) =>
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"status\":\"Healthy\"}", Encoding.UTF8, "application/json")
            };
        });

        var httpClientFactory = CreateHttpClientFactory(httpMessageHandler);
        var aggregator = new ApplicationHealthAggregator(
            httpClientFactory,
            MsOptions.Create(settings),
            Substitute.For<ILogger<ApplicationHealthAggregator>>());

        // Act
        var report = await aggregator.GetApplicationHealthAsync("TestApp");

        // Assert
        report.ShouldNotBeNull();
        report.Status.ShouldBe("Healthy");
        report.TotalCount.ShouldBe(2);
        report.HealthyCount.ShouldBe(2);
    }

    [Fact]
    public async Task parse_json_response_with_lowercase_status_field()
    {
        // Arrange
        var settings = new Settings();
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service1", Uri = "http://service1/health" });
        settings.Applications.Add("TestApp", new ApplicationConfiguration
        {
            Members = new List<string> { "Service1" }
        });

        var httpMessageHandler = new TestHttpMessageHandler((request) =>
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"status\":\"Healthy\"}", Encoding.UTF8, "application/json")
            };
        });

        var httpClientFactory = CreateHttpClientFactory(httpMessageHandler);
        var aggregator = new ApplicationHealthAggregator(
            httpClientFactory,
            MsOptions.Create(settings),
            Substitute.For<ILogger<ApplicationHealthAggregator>>());

        // Act
        var report = await aggregator.GetApplicationHealthAsync("TestApp");

        // Assert
        report.ShouldNotBeNull();
        report.Members.ShouldHaveSingleItem();
        report.Members[0].Status.ShouldBe("Healthy");
    }

    [Fact]
    public async Task parse_json_response_with_uppercase_status_field()
    {
        // Arrange
        var settings = new Settings();
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service1", Uri = "http://service1/health" });
        settings.Applications.Add("TestApp", new ApplicationConfiguration
        {
            Members = new List<string> { "Service1" }
        });

        var httpMessageHandler = new TestHttpMessageHandler((request) =>
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"Status\":\"Degraded\"}", Encoding.UTF8, "application/json")
            };
        });

        var httpClientFactory = CreateHttpClientFactory(httpMessageHandler);
        var aggregator = new ApplicationHealthAggregator(
            httpClientFactory,
            MsOptions.Create(settings),
            Substitute.For<ILogger<ApplicationHealthAggregator>>());

        // Act
        var report = await aggregator.GetApplicationHealthAsync("TestApp");

        // Assert
        report.ShouldNotBeNull();
        report.Members.ShouldHaveSingleItem();
        report.Members[0].Status.ShouldBe("Degraded");
    }

    [Fact]
    public async Task treat_200_without_json_as_healthy()
    {
        // Arrange
        var settings = new Settings();
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service1", Uri = "http://service1/health" });
        settings.Applications.Add("TestApp", new ApplicationConfiguration
        {
            Members = new List<string> { "Service1" }
        });

        var httpMessageHandler = new TestHttpMessageHandler((request) =>
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("OK", Encoding.UTF8, "text/plain")
            };
        });

        var httpClientFactory = CreateHttpClientFactory(httpMessageHandler);
        var aggregator = new ApplicationHealthAggregator(
            httpClientFactory,
            MsOptions.Create(settings),
            Substitute.For<ILogger<ApplicationHealthAggregator>>());

        // Act
        var report = await aggregator.GetApplicationHealthAsync("TestApp");

        // Assert
        report.ShouldNotBeNull();
        report.Members.ShouldHaveSingleItem();
        report.Members[0].Status.ShouldBe("Healthy");
    }

    [Fact]
    public async Task treat_500_as_unhealthy()
    {
        // Arrange
        var settings = new Settings();
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service1", Uri = "http://service1/health" });
        settings.Applications.Add("TestApp", new ApplicationConfiguration
        {
            Members = new List<string> { "Service1" }
        });

        var httpMessageHandler = new TestHttpMessageHandler((request) =>
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Error", Encoding.UTF8, "text/plain")
            };
        });

        var httpClientFactory = CreateHttpClientFactory(httpMessageHandler);
        var aggregator = new ApplicationHealthAggregator(
            httpClientFactory,
            MsOptions.Create(settings),
            Substitute.For<ILogger<ApplicationHealthAggregator>>());

        // Act
        var report = await aggregator.GetApplicationHealthAsync("TestApp");

        // Assert
        report.ShouldNotBeNull();
        report.Members.ShouldHaveSingleItem();
        report.Members[0].Status.ShouldBe("Unhealthy");
    }

    [Fact]
    public async Task treat_timeout_as_unreachable()
    {
        // Arrange
        var settings = new Settings();
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service1", Uri = "http://service1/health" });
        settings.Applications.Add("TestApp", new ApplicationConfiguration
        {
            Members = new List<string> { "Service1" }
        });

        var httpMessageHandler = new TestHttpMessageHandler((request) =>
        {
            throw new TaskCanceledException("Timeout");
        });

        var httpClientFactory = CreateHttpClientFactory(httpMessageHandler);
        var aggregator = new ApplicationHealthAggregator(
            httpClientFactory,
            MsOptions.Create(settings),
            Substitute.For<ILogger<ApplicationHealthAggregator>>());

        // Act
        var report = await aggregator.GetApplicationHealthAsync("TestApp");

        // Assert
        report.ShouldNotBeNull();
        report.Members.ShouldHaveSingleItem();
        report.Members[0].Status.ShouldBe("Unreachable");
        report.Members[0].Payload.ShouldNotBeNull();
        report.Members[0].Payload!.ShouldContain("Timeout");
    }

    [Fact]
    public async Task treat_network_exception_as_unreachable()
    {
        // Arrange
        var settings = new Settings();
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service1", Uri = "http://service1/health" });
        settings.Applications.Add("TestApp", new ApplicationConfiguration
        {
            Members = new List<string> { "Service1" }
        });

        var httpMessageHandler = new TestHttpMessageHandler((request) =>
        {
            throw new HttpRequestException("Network error");
        });

        var httpClientFactory = CreateHttpClientFactory(httpMessageHandler);
        var aggregator = new ApplicationHealthAggregator(
            httpClientFactory,
            MsOptions.Create(settings),
            Substitute.For<ILogger<ApplicationHealthAggregator>>());

        // Act
        var report = await aggregator.GetApplicationHealthAsync("TestApp");

        // Assert
        report.ShouldNotBeNull();
        report.Members.ShouldHaveSingleItem();
        report.Members[0].Status.ShouldBe("Unreachable");
        report.Members[0].Payload.ShouldNotBeNull();
        report.Members[0].Payload!.ShouldContain("Network error");
    }

    [Fact]
    public async Task return_null_for_nonexistent_application()
    {
        // Arrange
        var settings = new Settings();
        var httpClientFactory = CreateHttpClientFactory(new TestHttpMessageHandler(_ => new HttpResponseMessage()));
        var aggregator = new ApplicationHealthAggregator(
            httpClientFactory,
            MsOptions.Create(settings),
            Substitute.For<ILogger<ApplicationHealthAggregator>>());

        // Act
        var report = await aggregator.GetApplicationHealthAsync("NonExistent");

        // Assert
        report.ShouldBeNull();
    }

    [Fact]
    public async Task get_all_applications_returns_all_configured_applications()
    {
        // Arrange
        var settings = new Settings();
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service1", Uri = "http://service1/health" });
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service2", Uri = "http://service2/health" });
        settings.Applications.Add("App1", new ApplicationConfiguration
        {
            Members = new List<string> { "Service1" }
        });
        settings.Applications.Add("App2", new ApplicationConfiguration
        {
            Members = new List<string> { "Service2" }
        });

        var httpMessageHandler = new TestHttpMessageHandler((request) =>
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"status\":\"Healthy\"}", Encoding.UTF8, "application/json")
            };
        });

        var httpClientFactory = CreateHttpClientFactory(httpMessageHandler);
        var aggregator = new ApplicationHealthAggregator(
            httpClientFactory,
            MsOptions.Create(settings),
            Substitute.For<ILogger<ApplicationHealthAggregator>>());

        // Act
        var reports = await aggregator.GetAllApplicationsHealthAsync();

        // Assert
        reports.ShouldNotBeNull();
        reports.Count.ShouldBe(2);
        reports.Select(r => r.Name).ShouldContain("App1");
        reports.Select(r => r.Name).ShouldContain("App2");
    }

    [Fact]
    public async Task calculate_average_duration_correctly()
    {
        // Arrange
        var settings = new Settings();
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service1", Uri = "http://service1/health" });
        settings.HealthChecks.Add(new HealthCheckSetting { Name = "Service2", Uri = "http://service2/health" });
        settings.Applications.Add("TestApp", new ApplicationConfiguration
        {
            Members = new List<string> { "Service1", "Service2" }
        });

        var httpMessageHandler = new TestHttpMessageHandler((request) =>
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"status\":\"Healthy\"}", Encoding.UTF8, "application/json")
            };
        });

        var httpClientFactory = CreateHttpClientFactory(httpMessageHandler);
        var aggregator = new ApplicationHealthAggregator(
            httpClientFactory,
            MsOptions.Create(settings),
            Substitute.For<ILogger<ApplicationHealthAggregator>>());

        // Act
        var report = await aggregator.GetApplicationHealthAsync("TestApp");

        // Assert
        report.ShouldNotBeNull();
        report.AverageDurationMs.ShouldBeGreaterThanOrEqualTo(0);
        report.Members.Count.ShouldBe(2);
        report.Members.All(m => m.DurationMs >= 0).ShouldBeTrue();
    }

    private static IHttpClientFactory CreateHttpClientFactory(HttpMessageHandler messageHandler)
    {
        var httpClient = new HttpClient(messageHandler);
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);
        return httpClientFactory;
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFunc;

        public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFunc)
        {
            _responseFunc = responseFunc;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responseFunc(request));
        }
    }
}

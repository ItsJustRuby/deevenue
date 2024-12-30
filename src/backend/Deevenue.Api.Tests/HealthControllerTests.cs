using System.Net;
using Deevenue.Infrastructure.Db;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;

namespace Deevenue.Api.Tests;

public class DeevenueApiTestsWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    private const string PostgresIdentity = "deevenue-tests";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17.1-alpine3.20")
        .WithPortBinding(5432, true)
        .WithUsername(PostgresIdentity)
        .WithPassword(PostgresIdentity)
        .WithDatabase(PostgresIdentity)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    private const string StorageIdentity = "Qo4umcKVGWpD3p0nwobo";

    private readonly MinioContainer _minio = new MinioBuilder()
        .WithImage("minio/minio:RELEASE.2024-11-07T00-52-20Z")
        .WithPortBinding(9000, true)
        .WithUsername(StorageIdentity)
        .WithPassword(StorageIdentity)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(9000))
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        static void set(string suffix, string value) =>
            Environment.SetEnvironmentVariable($"DEEVENUE_{suffix}", value);

        set("DEPLOYMENT", "tests");

        // TODO: Reuse in testing API client if required (it's localhost though so should be fine)
        set("AUTH_HEADER_NAME", "X-User");
        set("AUTH_HEADER_VALUE", "admin");

        set("MEDIA_OPERATION_TIMEOUT_MS", "10000");
        set("BACKUP_DIRECTORY", "/todo");

        set("EXTERNAL_SENTRY_DSN", string.Empty);
        set("EXTERNAL_SENTRY_TRACES_SAMPLE_RATE", "0.0");

        // TODO: Read https://dotnet.testcontainers.org/test_frameworks/xunit_net/

        set("DB_HOST", _postgres.Hostname);
        set("DB_PORT", _postgres.GetMappedPublicPort(5432).ToString());
        set("DB_DB", PostgresIdentity);
        set("DB_USER", PostgresIdentity);
        set("DB_PASSWORD", PostgresIdentity);

        set("STORAGE_ENDPOINT", $"{_minio.Hostname}:{_minio.GetMappedPublicPort(9000)}");
        set("STORAGE_ACCESS_KEY", StorageIdentity);
        set("STORAGE_SECRET_KEY", StorageIdentity);
    }

    public async Task SetupAsync()
    {
        await _postgres.StartAsync();
        await _minio.StartAsync();
        Console.WriteLine("Postgres started");
    }

    public async Task TeardownAsync()
    {
        await _postgres.DisposeAsync();
        await _minio.DisposeAsync();
    }
}

public class HealthControllerTests : IClassFixture<DeevenueApiTestsWebApplicationFactory<Program>>, IAsyncLifetime
{
    // TODO: Learn about xUnit fixtures again
    private readonly DeevenueApiTestsWebApplicationFactory<Program> _factory;

    public HealthControllerTests(
        DeevenueApiTestsWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public async Task DisposeAsync()
    {
        await _factory.TeardownAsync();
    }

    [Fact]
    public async Task HealthCheck_RespondsOk()
    {
        // TODO: Move this to a "once before all tests" fixture.
        await using AsyncServiceScope scope = _factory.Services.CreateAsyncScope();
        DeevenueContext dbContext = scope.ServiceProvider.GetRequiredService<DeevenueContext>();
        await dbContext.Database.MigrateAsync();

        var client = _factory.CreateClient();
        var result = await client.GetAsync("/health");
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    public Task InitializeAsync()
    {
        return _factory.SetupAsync();
    }
}

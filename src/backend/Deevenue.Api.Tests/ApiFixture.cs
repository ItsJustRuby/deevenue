using Deevenue.Api.Tests;
using Deevenue.Infrastructure.Db;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;
using Xunit.Runner.Common;
using Xunit.Sdk;

[assembly: AssemblyFixture(typeof(ApiFixture))]

namespace Deevenue.Api.Tests;

// TODO: https://dotnet.testcontainers.org/test_frameworks/xunit_net/
// isn't maximally helpful, but maybe you can use *some* of it?

// TODO: Connect ITestOutputHelper with ILogger? It would be great to have API
// log output on failure.
//builder.ConfigureServices(services =>
//{
//    services.AddLogging(logging => logging...)
//})
//
// or add exception handler for 500s that is only registered on "tests"?
public class ApiFixture : IDisposable
{
    private bool disposedValue;

    // It's not null, trust me. (It is set by magic [reflection]).
    internal static DeevenueApiTestsWebApplicationFactory Instance = null!;

    public ApiFixture(IMessageSink sink)
    {
        var factory = new DeevenueApiTestsWebApplicationFactory(sink);
        factory.SetupAsync().Wait();
        Instance = factory;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
                Instance.TeardownAsync().Wait();

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    internal class DeevenueApiTestsWebApplicationFactory(IMessageSink sink) : WebApplicationFactory<Program>
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

            // Not really used during testing since inter-Docker
            // connections are from localhost to localhost.
            set("AUTH_HEADER_NAME", "X-User");
            set("AUTH_HEADER_VALUE", "admin");

            set("MEDIA_OPERATION_TIMEOUT_MS", "10000");
            set("BACKUP_DIRECTORY", "/todo");

            set("EXTERNAL_SENTRY_DSN", string.Empty);
            set("EXTERNAL_SENTRY_TRACES_SAMPLE_RATE", "0.0");

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

            sink.OnMessage(new DiagnosticMessage("Migrating database..."));
            await using var scope = Services.CreateAsyncScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<DeevenueContext>();
            await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
            sink.OnMessage(new DiagnosticMessage("Database migrated."));
        }

        public async Task TeardownAsync()
        {
            await _postgres.DisposeAsync();
            await _minio.DisposeAsync();
            await DisposeAsync();
        }
    }
}

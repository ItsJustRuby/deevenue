using System.Net;
using Deevenue.Infrastructure.Db;
using dotenv.net;
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
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17.1-alpine3.20")
        // TODO: This seems to be irrelevant, clean it up in here and the .env file.
        //.WithHostname("deevenue-tests-db")
        .WithExposedPort(5432)
        // TODO: ", true" would apparently be nicer to do
        .WithPortBinding(5432, 5432)
        .WithUsername("deevenue-tests")
        .WithPassword("deevenue-tests")
        .WithDatabase("deevenue-tests")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    private readonly MinioContainer _minio = new MinioBuilder()
        .WithImage("minio/minio:RELEASE.2024-11-07T00-52-20Z")
        //.WithHostname("deevenue-storage")
        //// TODO: err
        ////.WithExposedPort(9000)
        //.WithUsername("Qo4umcKVGWpD3p0nwobo")
        //.WithPassword("E6WCdmbk6b52wsHgGIz6")
        //.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(9000))
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        DotEnv.Load(new DotEnvOptions(envFilePaths: ["./.env.tests"]));

        // TODO: Read https://dotnet.testcontainers.org/test_frameworks/xunit_net/

        // TODO: Aight this not work the other way around.
        Environment.SetEnvironmentVariable("DEEVENUE_DB_HOST", _postgres.Hostname);
        // TODO: This might be necessary
        //Environment.SetEnvironmentVariable("DEEVENUE_DB_PORT", _postgres.GetMappedPublicPort(5432).ToString());
    }

    public async Task SetupAsync()
    {
        //await _minio.StartAsync();
        await _postgres.StartAsync();
        Console.WriteLine("Postgres started");
    }

    public async Task TeardownAsync()
    {
        //await _minio.DisposeAsync();
        await _postgres.DisposeAsync();
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
        // TODO: Ok, we're there! Move this to a fixture.
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

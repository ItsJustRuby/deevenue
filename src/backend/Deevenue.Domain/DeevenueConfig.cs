using FluentValidation;

namespace Deevenue.Domain;

public sealed class DeevenueConfig
{
    public required DeevenueDbConfig Db { get; init; }
    public required DeevenueStorageConfig Storage { get; init; }
    public required DeevenueAuthConfig Auth { get; init; }

    public required DeevenueMediaConfig Media { get; init; }
    public required DeevenueBackupConfig Backup { get; init; }
    public required DeevenueExternalConfig External { get; init; }

    public required DeevenueEnvironment Environment { get; init; }
}

public sealed record DeevenueDbConfig
{
    public required string Host { get; init; }
    public required string Database { get; init; }
    public required int Port { get; init; }
    public required string User { get; init; }
    public required string Password { get; init; }
}

public sealed class DeevenueStorageConfig
{
    public required string Endpoint { get; init; }

    public required string AccessKey { get; init; }

    public required string SecretKey { get; init; }
}

public sealed class DeevenueAuthConfig
{
    public required string HeaderName { get; init; }

    public required string HeaderValue { get; init; }
}

public sealed class DeevenueMediaConfig
{
    public required int OperationTimeoutMs { get; init; }
}

public sealed class DeevenueBackupConfig
{
    public required string Directory { get; init; }
}
public sealed class DeevenueExternalConfig
{
    public required DeevenueSentryConfig Sentry { get; init; }
}
public sealed class DeevenueSentryConfig
{
    public required string Dsn { get; init; }
    public required double TracesSampleRate { get; init; }
}

public sealed record DeevenueEnvironment(
    bool AllowsSensitiveDataLogging,
    string Name,
    bool OffersOpenApi,
    bool RequiresSentryIntegration,
    bool RequiresScopedDbContextFactoryLifetime,
    bool SkipSchedulingJobs
)
{
    private static readonly DeevenueEnvironment production = new(
        AllowsSensitiveDataLogging: false,
        Name: "production",
        OffersOpenApi: false,
        RequiresSentryIntegration: true,
        RequiresScopedDbContextFactoryLifetime: false,
        SkipSchedulingJobs: false
    );
    private static readonly DeevenueEnvironment development = new(
        AllowsSensitiveDataLogging: true,
        Name: "development",
        OffersOpenApi: true,
        RequiresSentryIntegration: false,
        RequiresScopedDbContextFactoryLifetime: false,
        SkipSchedulingJobs: false
    );
    private static readonly DeevenueEnvironment tests = new(
        AllowsSensitiveDataLogging: true,
        Name: "tests",
        OffersOpenApi: true,
        RequiresSentryIntegration: false,
        RequiresScopedDbContextFactoryLifetime: true,
        SkipSchedulingJobs: true
    );

    internal static DeevenueEnvironment Match(string? configIdentifier)
    {
        return configIdentifier switch
        {
            "production" => production,
            "development" => development,
            "tests" => tests,
            _ => throw new ArgumentOutOfRangeException(nameof(configIdentifier)),
        };
    }
}

public static class StaticConfig
{
    public static DeevenueConfig Config => instance!;
    private static DeevenueConfig? instance;

    internal static void Bootstrap(IDependencyInjection dependencyInjection)
    {
        var config = dependencyInjection.Config;

        var result = new DeevenueConfig
        {
            Db = new DeevenueDbConfig
            {
                Host = config["DB_HOST"]!,
                Database = config["DB_DB"]!,
                Port = int.Parse(config["DB_PORT"]!),
                User = config["DB_USER"]!,
                Password = config["DB_PASSWORD"]!,
            },
            Storage = new DeevenueStorageConfig
            {
                Endpoint = config["STORAGE_ENDPOINT"]!,
                AccessKey = config["STORAGE_ACCESS_KEY"]!,
                SecretKey = config["STORAGE_SECRET_KEY"]!
            },
            Auth = new DeevenueAuthConfig
            {
                HeaderName = config["AUTH_HEADER_NAME"]!,
                HeaderValue = config["AUTH_HEADER_VALUE"]!
            },
            Media = new DeevenueMediaConfig
            {
                OperationTimeoutMs = int.Parse(config["MEDIA_OPERATION_TIMEOUT_MS"]!)
            },
            Backup = new DeevenueBackupConfig
            {
                Directory = config["BACKUP_DIRECTORY"]!
            },
            External = new DeevenueExternalConfig
            {
                Sentry = new DeevenueSentryConfig
                {
                    Dsn = config["EXTERNAL_SENTRY_DSN"]!,
                    TracesSampleRate = double.Parse(config["EXTERNAL_SENTRY_TRACES_SAMPLE_RATE"]!)
                }
            },
            Environment = DeevenueEnvironment.Match(config["DEPLOYMENT"])
        };

        new ConfigValidator(result).ValidateAndThrow(result);
        instance = result;
    }

    private class ConfigValidator : AbstractValidator<DeevenueConfig>
    {
        public ConfigValidator(DeevenueConfig config)
        {
            RuleFor(c => c.Db).SetValidator(new Db());
            RuleFor(c => c.Storage).SetValidator(new Storage());
            RuleFor(c => c.Auth).SetValidator(new Auth());
            RuleFor(c => c.Media).SetValidator(new Media());
            RuleFor(c => c.Backup).SetValidator(new Backup());
            RuleFor(c => c.External).SetValidator(new External(config));
        }

        private class Db : AbstractValidator<DeevenueDbConfig>
        {
            public Db()
            {
                // This is probably a bit stringent, but works for the current setup.
                RuleFor(c => c.Host).NotEmpty();
                RuleFor(c => c.Database).NotEmpty();
                RuleFor(c => c.User).NotEmpty();
                RuleFor(c => c.Password).NotEmpty();
            }
        }

        private class Storage : AbstractValidator<DeevenueStorageConfig>
        {
            public Storage()
            {
                // Important: Endpoint needs to contain both a host and a port.
                RuleFor(c => c.Endpoint).NotEmpty().Matches(@"^(.*?):(\d+)$");
                RuleFor(c => c.AccessKey).NotEmpty();
                RuleFor(c => c.SecretKey).NotEmpty();
            }
        }

        private class Auth : AbstractValidator<DeevenueAuthConfig>
        {
            public Auth()
            {
                RuleFor(c => c.HeaderName).NotEmpty();
                RuleFor(c => c.HeaderValue).NotEmpty();
            }
        }

        private class Media : AbstractValidator<DeevenueMediaConfig>
        {
            public Media()
            {
                RuleFor(c => c.OperationTimeoutMs).NotEmpty().GreaterThan(0);
            }
        }

        private class Backup : AbstractValidator<DeevenueBackupConfig>
        {
            public Backup()
            {
                RuleFor(c => c.Directory).NotEmpty();
            }
        }

        private class External : AbstractValidator<DeevenueExternalConfig>
        {
            public External(DeevenueConfig config)
            {
                RuleFor(c => c.Sentry).SetValidator(new Sentry(config));
            }
        }

        private class Sentry : AbstractValidator<DeevenueSentryConfig>
        {
            public Sentry(DeevenueConfig config)
            {
                RuleFor(c => c.Dsn).NotEmpty().When(c => config.Environment.RequiresSentryIntegration);
                RuleFor(c => c.TracesSampleRate).InclusiveBetween(0.0, 1.0);
            }
        }
    }
}

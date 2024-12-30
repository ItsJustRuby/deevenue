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

    public required IDeevenueEnvironment Environment { get; init; }
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
    public required string Environment { get; init; }
    public required double TracesSampleRate { get; init; }
}

public interface IDeevenueEnvironment
{
    bool AllowsSensitiveDataLogging { get; }
    bool OffersOpenApi { get; }
    bool RequiresSentryIntegration { get; }
    bool RequiresScopedDbContextFactoryLifetime { get; }
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
                    Environment = config["DEPLOYMENT"]!,
                    TracesSampleRate = double.Parse(config["EXTERNAL_SENTRY_TRACES_SAMPLE_RATE"]!)
                }
            },
            Environment = Environment.Match(config["DEPLOYMENT"])
        };

        new ConfigValidator().ValidateAndThrow(result);
        instance = result;
    }

    private static class Environment
    {
        private static readonly Env production = new(
            AllowsSensitiveDataLogging: false,
            OffersOpenApi: false,
            RequiresSentryIntegration: true,
            RequiresScopedDbContextFactoryLifetime: false
        );
        private static readonly Env development = new(
            AllowsSensitiveDataLogging: true,
            OffersOpenApi: true,
            RequiresSentryIntegration: false,
            RequiresScopedDbContextFactoryLifetime: false
        );
        private static readonly Env tests = new(
            AllowsSensitiveDataLogging: true,
            OffersOpenApi: true,
            RequiresSentryIntegration: false,
            RequiresScopedDbContextFactoryLifetime: true
        );

        public static IDeevenueEnvironment Match(string? configIdentifier)
        {
            return configIdentifier switch
            {
                "production" => production,
                "development" => development,
                "tests" => tests,
                _ => throw new ArgumentOutOfRangeException(nameof(configIdentifier)),
            };
        }

        private record Env(
            bool AllowsSensitiveDataLogging,
            bool OffersOpenApi,
            bool RequiresSentryIntegration,
            bool RequiresScopedDbContextFactoryLifetime
        ) : IDeevenueEnvironment;
    }

    private class ConfigValidator : AbstractValidator<DeevenueConfig>
    {
        public ConfigValidator()
        {
            RuleFor(c => c.Db).SetValidator(new Db());
            RuleFor(c => c.Storage).SetValidator(new Storage());
            RuleFor(c => c.Auth).SetValidator(new Auth());
            RuleFor(c => c.Media).SetValidator(new Media());
            RuleFor(c => c.Backup).SetValidator(new Backup());
            RuleFor(c => c.External).SetValidator(new External());
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
            public External()
            {
                RuleFor(c => c.Sentry).SetValidator(new Sentry());
            }
        }

        private class Sentry : AbstractValidator<DeevenueSentryConfig>
        {
            public Sentry()
            {
                RuleFor(c => c.Environment).NotEmpty();
                // TODO: Consider making .Environment hella typesafe
                RuleFor(c => c.Dsn).NotEmpty().When(c => c.Environment == "production");
                RuleFor(c => c.TracesSampleRate).InclusiveBetween(0.0, 1.0);
            }
        }
    }
}

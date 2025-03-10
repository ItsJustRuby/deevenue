using Deevenue.Api;
using Deevenue.Api.Framework;
using Deevenue.Domain;
using Deevenue.Domain.Thumbnails;
using Deevenue.Infrastructure;
using Deevenue.Infrastructure.Db;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Quartz.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(prefix: "DEEVENUE_");
builder.Services.AddControllers(c =>
{
    var jsonFormatter = c.OutputFormatters
        .Single(f => f is SystemTextJsonOutputFormatter) as SystemTextJsonOutputFormatter;
    c.OutputFormatters.Insert(0, new NotificationViewModelFormatter(jsonFormatter!));

    c.Filters.Add<FluentValidationExceptionFilter>();
});
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi(o =>
{
    // This shuts up the linter, and is valid - let us be explicit that we do not have
    // any security (on the application level) whatsoever.
    o.AddDocumentTransformer((document, _, _) =>
    {
        document.SecurityRequirements = [[]];
        return Task.CompletedTask;
    });

    // For polymorphism, make the type discriminator property not only required
    // on the (interface) parent type in the resulting OpenAPI Schema,
    // explicitly also flag it as required on all implementations.
    // This fixes OpenAPI client generators that do not automatically understand that.
    o.AddSchemaTransformer((schema, _, _) =>
    {
        if (schema.Discriminator == null)
            return Task.CompletedTask;

        foreach (var descendant in schema.AnyOf)
            descendant.Required.Add(schema.Discriminator.PropertyName);

        return Task.CompletedTask;
    });
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(600);
    options.Cookie.Name = "DeevenueSessionCookie";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddRequestDecompression();

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;

    options.ConstraintMap.Add(nameof(ThumbnailSizeAbbreviation), typeof(ThumbnailSizeConstraint));
    options.ConstraintMap.Add(nameof(Rating), typeof(RatingConstraint));
});

if (Is.OnlyGeneratingOpenAPIDefinitions)
{
    builder.Build().Run();
    return;
}

// You can safely ignore the warning here.
builder.Services.AddDataProtection().PersistKeysToDbContext<DeevenueContext>();

builder.Services.AddQuartzServer(o =>
{
    o.WaitForJobsToComplete = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddDeevenueApi();

builder.WebHost.UseSentry(c =>
{
    c.Debug = Config.Environment.AllowsSensitiveDataLogging;
    c.Dsn = Config.External.Sentry.Dsn;
    c.Environment = Config.Environment.Name;
    c.TracesSampleRate = Config.External.Sentry.TracesSampleRate;
    c.SetBeforeSend(sentryEvent =>
    {
        if (sentryEvent.SentryExceptions == null)
            return sentryEvent;

        foreach (var exception in sentryEvent.SentryExceptions)
        {
            if (exception.Mechanism?.Handled != true)
                return sentryEvent;
        }

        // If the event has exceptions,
        // but they are all handled,
        // there is no need to report them.
        return null;
    });
});

if (Config.Environment.AllowsSensitiveDataLogging)
    builder.Services.AddExceptionHandler<VerboseExceptionHandler>();

var app = builder.Build();

if (Config.Environment.OffersOpenApi)
    app.MapOpenApi();

app.UseRequestDecompression();

if (Config.Environment.AllowsSensitiveDataLogging)
    app.UseExceptionHandler();

app.UseMiddleware<HeaderAuthMiddleware>();
app.UseSession();
app.MapControllers();
app.UseInfrastructure();

app.Run();

// Explicitly giving this type a name enables it to be used in test fixtures
internal partial class Program { }

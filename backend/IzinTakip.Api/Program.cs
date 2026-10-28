using System.Globalization;
using System.Text.Json.Serialization;
using FluentValidation;
using IzinTakip.Api.Common.Auth;
using IzinTakip.Api.Common.Errors;
using IzinTakip.Api.Common.Validation;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, logger) =>
    {
        logger.ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.File(
                new CompactJsonFormatter(),
                Path.Combine(builder.Environment.ContentRootPath, "logs", "api-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30);

        if (builder.Environment.IsDevelopment())
        {
            logger.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {TraceId}{NewLine}{Exception}");
        }
        else
        {
            logger.WriteTo.Console(new CompactJsonFormatter());
        }
    });

    var connectionString = builder.Configuration.GetConnectionString("IzinTakip")
        ?? throw new InvalidOperationException("ConnectionStrings:IzinTakip tanımlı değil. Bağlantı dizesini user-secrets ya da ortam değişkeniyle verin.");

    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IGecerliKullanici, HttpGecerliKullanici>();
    builder.Services.AddScoped<AuditInterceptor>();
    builder.Services.AddDbContext<IzinTakipDbContext>((serviceProvider, options) =>
        options.UseSqlServer(connectionString)
               .AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>()));
    builder.Services.AddHealthChecks().AddDbContextCheck<IzinTakipDbContext>();

    builder.Services.AddProblemDetails(options =>
        options.CustomizeProblemDetails = context =>
            context.ProblemDetails.Extensions["traceId"] = GlobalExceptionHandler.TraceId(context.HttpContext));
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("tr");
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
        .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = ValidationFilter.HataYaniti)
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.MapHealthChecks("/health");
    app.MapControllers();

    app.Run();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "Uygulama baslatilamadi");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}

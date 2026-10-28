using System.Globalization;
using System.Security.Claims;
using System.Text.Json.Serialization;
using FluentValidation;
using IzinTakip.Api.BackgroundServices;
using IzinTakip.Api.Common;
using IzinTakip.Api.Common.Auth;
using IzinTakip.Api.Common.Bildirimler;
using IzinTakip.Api.Common.Errors;
using IzinTakip.Api.Common.Izin;
using IzinTakip.Api.Common.Kapsam;
using IzinTakip.Api.Common.Validation;
using IzinTakip.Infrastructure.Data;
using IzinTakip.Infrastructure.Data.Seed;
using IzinTakip.Infrastructure.Email;
using IzinTakip.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Context;
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
                Path.Combine(builder.Configuration["LogKlasoru"] ?? Path.Combine(builder.Environment.ContentRootPath, "logs"), "api-.log"),
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

    var jwtAyarlari = builder.Configuration.GetSection("Jwt").Get<JwtAyarlari>() ?? new JwtAyarlari();
    if (jwtAyarlari.SigningKey.Length < 32)
    {
        throw new InvalidOperationException("Jwt:SigningKey en az 32 karakter olmalı. Değeri user-secrets ya da ortam değişkeniyle verin.");
    }

    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddSingleton(jwtAyarlari);
    builder.Services.AddMemoryCache();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IGecerliKullanici, HttpGecerliKullanici>();
    builder.Services.AddScoped<KullaniciYetkiOkuyucu>();
    builder.Services.AddScoped<AuditInterceptor>();
    builder.Services.AddScoped<BaslangicVerisi>();
    builder.Services.AddDbContext<IzinTakipDbContext>((serviceProvider, options) =>
        options.UseSqlServer(connectionString)
               .AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>()));
    builder.Services.AddHealthChecks().AddDbContextCheck<IzinTakipDbContext>();

    builder.Services.AddIdentityCore<Kullanici>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        })
        .AddRoles<Rol>()
        .AddEntityFrameworkStores<IzinTakipDbContext>()
        .AddSignInManager()
        .AddErrorDescriber<TurkceIdentityErrorDescriber>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtAyarlari.Issuer,
                ValidAudience = jwtAyarlari.Audience,
                IssuerSigningKey = jwtAyarlari.ImzaAnahtari(),
                NameClaimType = JwtRegisteredClaimNames.Sub,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var okuyucu = context.HttpContext.RequestServices.GetRequiredService<KullaniciYetkiOkuyucu>();
                    var yetkiler = int.TryParse(context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var kullaniciId)
                        ? await okuyucu.OkuAsync(kullaniciId, context.HttpContext.RequestAborted)
                        : null;
                    if (yetkiler is not { Aktif: true })
                    {
                        context.Fail("Kullanici bulunamadi ya da pasif.");
                    }
                }
            };
        });
    builder.Services.AddAuthorizationBuilder()
        .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
    builder.Services.AddScoped<IAuthorizationHandler, YetkiAuthorizationHandler>();
    builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, YetkiSonucuHandler>();

    builder.Services.AddProblemDetails(options =>
        options.CustomizeProblemDetails = context =>
            context.ProblemDetails.Extensions["traceId"] = GlobalExceptionHandler.TraceId(context.HttpContext));
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("tr");
    ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    var saatDilimi = TimeZoneInfo.FindSystemTimeZoneById(builder.Configuration["Sirket:SaatDilimi"] ?? "Europe/Istanbul");
    builder.Services.AddSingleton(serviceProvider => new SirketSaati(serviceProvider.GetRequiredService<TimeProvider>(), saatDilimi));
    builder.Services.AddScoped<CalisanKapsami>();
    builder.Services.AddScoped<BakiyeSorgusu>();
    builder.Services.AddScoped<CalismaTakvimiOkuyucu>();
    builder.Services.AddScoped<OnayciBulucu>();
    builder.Services.AddScoped<BildirimYazici>();

    builder.Services.AddSingleton(builder.Configuration.GetSection("Uygulama").Get<UygulamaAyarlari>() ?? new UygulamaAyarlari());
    builder.Services.AddSingleton(builder.Configuration.GetSection("Email").Get<EmailAyarlari>() ?? new EmailAyarlari());
    builder.Services.AddSingleton<IEmailQueue, ChannelEmailQueue>();
    builder.Services.AddSingleton<IEmailSender, MailKitEmailSender>();
    builder.Services.AddHostedService<EmailGonderimServisi>();
    builder.Services.AddSingleton(builder.Configuration.GetSection("ArkaPlan").Get<ArkaPlanAyarlari>() ?? new ArkaPlanAyarlari());
    builder.Services.AddHostedService<YillikHakServisi>();
    builder.Services.AddHostedService<HatirlatmaServisi>();
    foreach (var serviceTipi in typeof(Program).Assembly.GetTypes().Where(t =>
                 t is { IsClass: true, IsAbstract: false }
                 && t.Name.EndsWith("Service", StringComparison.Ordinal)
                 && t.Namespace?.StartsWith("IzinTakip.Api.Features", StringComparison.Ordinal) == true))
    {
        builder.Services.AddScoped(serviceTipi);
    }

    builder.Services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        })
        .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = ValidationFilter.HataYaniti)
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };
        document.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = [] }];
        return Task.CompletedTask;
    }));

    var app = builder.Build();

    var migrationCalistir = app.Configuration.GetValue<bool>("Database:MigrateOnStartup");
    var baslangicVerisiYukle = app.Configuration.GetValue<bool>("Database:SeedOnStartup");
    if (migrationCalistir || baslangicVerisiYukle)
    {
        await using var scope = app.Services.CreateAsyncScope();
        if (migrationCalistir)
        {
            await scope.ServiceProvider.GetRequiredService<IzinTakipDbContext>().Database.MigrateAsync();
        }

        if (baslangicVerisiYukle)
        {
            await scope.ServiceProvider.GetRequiredService<BaslangicVerisi>().HazirlaAsync(
                new BaslangicVerisiAyarlari(
                    app.Configuration["Seed:SuperAdmin:Eposta"],
                    app.Configuration["Seed:SuperAdmin:Parola"],
                    app.Configuration.GetValue<bool>("Seed:DemoVeri"),
                    app.Configuration["Seed:Demo:Parola"]),
                CancellationToken.None);
        }
    }

    app.UseSerilogRequestLogging(options => options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        diagnosticContext.Set("KullaniciId", httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)));
    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseAuthentication();
    app.Use(async (context, next) =>
    {
        using (LogContext.PushProperty("KullaniciId", context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)))
        {
            await next(context);
        }
    });
    app.UseAuthorization();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference().AllowAnonymous();
    }

    app.MapHealthChecks("/health").AllowAnonymous();
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

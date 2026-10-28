using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading.RateLimiting;
using IzinTakip.Gateway.Bff;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Serilog;
using Serilog.Formatting.Compact;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

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
                Path.Combine(builder.Configuration["LogKlasoru"] ?? Path.Combine(builder.Environment.ContentRootPath, "logs"), "gateway-.log"),
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

    var apiAdresi = builder.Configuration["Api:Adres"]
        ?? throw new InvalidOperationException("Api:Adres tanımlı değil. Api'nin adresini appsettings ya da ortam değişkeniyle verin.");

    builder.Services.AddDataProtection()
        .SetApplicationName("IzinTakip.Gateway")
        .PersistKeysToFileSystem(new DirectoryInfo(
            builder.Configuration["DataProtection:Klasor"] ?? Path.Combine(builder.Environment.ContentRootPath, "keys")));
    builder.Services.AddSingleton<OturumCerezi>();
    builder.Services.AddHttpClient("api", client => client.BaseAddress = new Uri(apiAdresi));
    builder.Services.AddHealthChecks();

    builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;
        context.ProblemDetails.Extensions["traceId"] = traceId;
        context.ProblemDetails.Detail ??= context.ProblemDetails.Status switch
        {
            StatusCodes.Status404NotFound => "Aradığınız adres bulunamadı.",
            StatusCodes.Status502BadGateway or StatusCodes.Status503ServiceUnavailable or StatusCodes.Status504GatewayTimeout =>
                "Sunucuya şu anda ulaşılamıyor. Biraz sonra tekrar deneyin.",
            >= StatusCodes.Status500InternalServerError => $"Beklenmeyen bir hata oluştu. Destek için bu kodu iletin: {traceId}",
            _ => null
        };
    });

    builder.Services.AddRateLimiter(options =>
    {
        var dakikadaDeneme = builder.Configuration.GetValue("GirisSiniri:DakikadaDeneme", 5);
        options.AddPolicy("giris", httpContext => RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "bilinmiyor",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = dakikadaDeneme, Window = TimeSpan.FromMinutes(1) }));
        options.OnRejected = async (context, _) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context.HttpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Detail = "Çok fazla giriş denemesi yapıldı. Bir dakika sonra tekrar deneyin."
                }
            });
        };
    });

    builder.Services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
        .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = context =>
            new BadRequestObjectResult(context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>().CreateProblemDetails(
                context.HttpContext,
                StatusCodes.Status400BadRequest,
                detail: "Gönderilen bilgiler okunamadı. Alanların biçimini kontrol edin.")));

    builder.Services.AddReverseProxy()
        .LoadFromMemory(
            [new RouteConfig { RouteId = "api", ClusterId = "api", Match = new RouteMatch { Path = "/api/{**yol}" } }],
            [
                new ClusterConfig
                {
                    ClusterId = "api",
                    Destinations = new Dictionary<string, DestinationConfig> { ["api"] = new() { Address = apiAdresi } }
                }
            ])
        .AddTransforms(context => context.AddRequestTransform(donusum =>
        {
            donusum.ProxyRequest.Headers.Remove("Cookie");
            donusum.ProxyRequest.Headers.Authorization =
                donusum.HttpContext.RequestServices.GetRequiredService<OturumCerezi>().Oku(donusum.HttpContext.Request) is { } token
                    ? new AuthenticationHeaderValue("Bearer", token)
                    : null;
            return ValueTask.CompletedTask;
        }));

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler(new ExceptionHandlerOptions
    {
        StatusCodeSelector = exception => exception is HttpRequestException
            ? StatusCodes.Status502BadGateway
            : StatusCodes.Status500InternalServerError
    });
    app.UseStatusCodePages();
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.Use(async (context, next) =>
    {
        var basliklar = context.Response.Headers;
        basliklar.XContentTypeOptions = "nosniff";
        basliklar.XFrameOptions = "DENY";
        basliklar["Referrer-Policy"] = "no-referrer";
        basliklar.ContentSecurityPolicy =
            "default-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; frame-ancestors 'none'; object-src 'none'; base-uri 'self'";
        await next(context);
    });
    app.UseStaticFiles();
    app.UseRateLimiter();
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/api/auth/login"))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        await next(context);
    });
    app.Use(async (context, next) =>
    {
        if ((context.Request.Path.StartsWithSegments("/api") || context.Request.Path.StartsWithSegments("/bff"))
            && context.Request.Headers["X-CSRF"] != "1")
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Detail = "İstek doğrulanamadı. Sayfayı yenileyip tekrar deneyin."
                }
            });
            return;
        }

        await next(context);
    });

    app.MapHealthChecks("/health");
    app.MapControllers();
    app.MapReverseProxy();
    app.MapFallbackToFile("index.html");

    app.Run();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "Gateway baslatilamadi");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}

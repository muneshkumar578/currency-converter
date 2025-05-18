using CurrencyConverter.Api.Middleware;
using CurrencyConverter.Api.Policies;
using CurrencyConverter.Contract.ExchangeRate;
using CurrencyConverter.Contract.Services;
using CurrencyConverter.Contract.User;
using CurrencyConverter.Dto.Shared;
using CurrencyConverter.Infrastructure.ExchangeRateProviders;
using CurrencyConverter.Infrastructure.Factories;
using CurrencyConverter.Service.Currency;
using CurrencyConverter.Service.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog for logging
ProgramHelper.AddLogging(builder);

// Add Serilog to web host
builder.Host.UseSerilog();

Log.Information("Starting Currency Converter application");

// Load configuration
var configuration = builder.Configuration;
configuration.AddJsonFile("appsettings.json", optional: false);
configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
ConfigDto config = configuration.Get<ConfigDto>()!;


// Set Http Client
ProgramHelper.AddHttpClient(builder, config);

// Add Services
ProgramHelper.RegisterServices(builder, configuration, config);
ProgramHelper.RegisterAuthentication(builder, config);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Rate Limiter
ProgramHelper.AddRateLimiter(builder, config);


// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;  // Header info for clients to see the supported versions
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global exception handler
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
        await GenericApiErrorHandler.HandleErrorAsync(context));
});

app.UseHttpsRedirection();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

// Logging and Monitoring middleware
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();

app.Run();


public static class ProgramHelper
{
    public static void RegisterServices(WebApplicationBuilder builder, ConfigurationManager configuration, ConfigDto config)
    {
        builder.Services.AddMemoryCache();

        builder.Services.AddSingleton(config);
        builder.Services.AddTransient<FrankfurterProvider>();
        builder.Services.AddSingleton<IExchangeRateProviderFactory, ExchangeRateProviderFactory>();
        builder.Services.AddScoped<ICurrencyService, CurrencyService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ITokenService, JwtTokenService>();
    }

    public static void AddHttpClient(WebApplicationBuilder builder, ConfigDto config)
    {
        var retryPolicy = RetryPolicy.GetRetryPolicy();
        var circuitBreakerPolicy = CircuitBreakerPolicy.GetCircuitBreakerPolicy();

        builder.Services
            .AddHttpClient(name: config.ExchangeRateProviderConfig.ClientName, client =>
            {
                client.BaseAddress = new Uri(config.ExchangeRateProviderConfig.BaseUrl);
            })
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(circuitBreakerPolicy);
    }

    public static void RegisterAuthentication(WebApplicationBuilder builder, ConfigDto config)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(configureOptions: c =>
            {
                c.RequireHttpsMetadata = true;
                c.SaveToken = true;
                c.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config.JwtConfig.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.JwtConfig.Secret)),
                    ValidAudience = config.JwtConfig.Audience,
                    ClockSkew = TimeSpan.Zero
                };
            });
    }

    public static void AddRateLimiter(WebApplicationBuilder builder, ConfigDto config)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    // Using the name of the user as the partition key for demonstration purposes.
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = config.RateLimitConfig.MaxRequestsInWindow,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(config.RateLimitConfig.WindowInMinutes)
                    }));
        });
    }

    public static void AddLogging(WebApplicationBuilder builder) 
    {
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration) // Read from appsettings.json
            .Enrich.FromLogContext()
            //.WriteTo.Console() // Write logs to console
            //.WriteTo.Seq(builder.Configuration["Serilog:SeqServerUrl"] ?? "http://localhost:5341") // Write to Seq server
            .CreateLogger();

        Log.Logger = logger;
    }
}

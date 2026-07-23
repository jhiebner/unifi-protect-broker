using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProtectBroker.Infrastructure.Data;
using ProtectBroker.Infrastructure.Services;
using ProtectBroker.Api.Hubs;
using ProtectBroker.Worker.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/protect-broker-.txt", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddSignalR();

    // Configure CORS
    var corsOrigins = builder.Configuration["Api:Cors:AllowedOrigins"]?.Split(',') 
        ?? new[] { "http://localhost:5173", "http://localhost:3000" };
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(corsOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // Configure Database
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString,
            sqlOptions => sqlOptions.CommandTimeout(30)));

    // Configure JWT Authentication
    var jwtKey = builder.Configuration["Jwt:Key"];
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];

    if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
    {
        throw new InvalidOperationException("JWT key must be configured and at least 32 characters long");
    }

    var key = Encoding.ASCII.GetBytes(jwtKey);
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        
        // Enable SignalR JWT token from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    (context.HttpContext.WebSockets.IsWebSocketRequest || 
                     context.Request.Headers.ContainsKey("Upgrade")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    // Register Protect API Configuration
    var protectConfig = new ProtectApiConfiguration();
    builder.Configuration.GetSection("UnifiProtect").Bind(protectConfig);
    builder.Services.AddSingleton(protectConfig);

    // Register Protect API Services
    builder.Services.AddScoped<IProtectApiClient, ProtectApiClient>();
    builder.Services.AddSingleton<IProtectWebSocketClient, ProtectWebSocketClient>();

    // Register Background Services
    builder.Services.AddHostedService<ProtectDeviceSyncService>();

    var app = builder.Build();

    // Configure middleware
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");
    
    // Map SignalR hubs
    app.MapHub<DeviceHub>("/signalr/devices");

    // Migrate database automatically
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migration completed");
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

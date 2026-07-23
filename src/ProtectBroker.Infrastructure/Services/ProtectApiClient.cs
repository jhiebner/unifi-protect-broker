using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ProtectBroker.Core.Models;
using Serilog;

namespace ProtectBroker.Infrastructure.Services;

/// <summary>
/// HTTP client for UniFi Protect API communication.
/// Handles authentication, request signing, and API calls.
/// </summary>
public interface IProtectApiClient
{
    Task<BootstrapResponse?> GetBootstrapAsync(CancellationToken cancellationToken = default);
    Task<bool> AuthenticateAsync(CancellationToken cancellationToken = default);
    Task RefreshTokenAsync(CancellationToken cancellationToken = default);
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default);
}

public class ProtectApiClient : IProtectApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ProtectApiConfiguration _config;
    private readonly ILogger _logger = Log.ForContext<ProtectApiClient>();
    
    private string? _accessKey;
    private string? _secretKey;
    private string? _userId;
    private DateTime _tokenExpiration = DateTime.MinValue;

    public ProtectApiClient(HttpClient httpClient, ProtectApiConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
        
        // Configure base address and settings
        _httpClient.BaseAddress = new Uri($"https://{config.Host}:{config.Port}");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        // Disable SSL verification if configured (not recommended for production)
        if (!config.VerifySsl)
        {
            _logger.Warning("SSL verification disabled for Protect API - not recommended for production");
        }
    }

    /// <summary>
    /// Authenticate with UniFi Protect using username and password.
    /// </summary>
    public async Task<bool> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Authenticating with UniFi Protect at {Host}:{Port}", 
                _config.Host, _config.Port);

            var loginPayload = new
            {
                username = _config.Username,
                password = _config.Password,
                rememberMe = true
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/api/auth/login", 
                loginPayload, 
                cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("Authentication failed: {StatusCode} {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                return false;
            }

            var authResponse = await response.Content.ReadAsAsync<AuthResponse>(cancellationToken);
            _accessKey = authResponse?.AccessKey;
            _secretKey = authResponse?.SecretKey;
            _userId = authResponse?.UserId;
            _tokenExpiration = DateTime.UtcNow.AddHours(24);

            _logger.Information("Successfully authenticated with UniFi Protect");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Authentication error");
            return false;
        }
    }

    /// <summary>
    /// Refresh authentication token.
    /// </summary>
    public async Task RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(_accessKey))
            {
                _logger.Warning("No access key to refresh, attempting full authentication");
                await AuthenticateAsync(cancellationToken);
                return;
            }

            _logger.Information("Refreshing UniFi Protect authentication token");

            var response = await _httpClient.PostAsync(
                "/api/auth/refresh",
                null,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Warning("Token refresh failed: {StatusCode}, re-authenticating", 
                    response.StatusCode);
                await AuthenticateAsync(cancellationToken);
                return;
            }

            var authResponse = await response.Content.ReadAsAsync<AuthResponse>(cancellationToken);
            _accessKey = authResponse?.AccessKey;
            _secretKey = authResponse?.SecretKey;
            _tokenExpiration = DateTime.UtcNow.AddHours(24);

            _logger.Information("Token refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Token refresh error");
            await AuthenticateAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Get bootstrap data containing all devices and configuration.
    /// </summary>
    public async Task<BootstrapResponse?> GetBootstrapAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if token needs refresh
            if (DateTime.UtcNow > _tokenExpiration.AddMinutes(-5))
            {
                await RefreshTokenAsync(cancellationToken);
            }

            if (string.IsNullOrEmpty(_accessKey))
            {
                _logger.Warning("Not authenticated, attempting authentication");
                if (!await AuthenticateAsync(cancellationToken))
                {
                    return null;
                }
            }

            _logger.Debug("Fetching bootstrap data from UniFi Protect");

            var response = await _httpClient.GetAsync(
                "/api/bootstrap",
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("Bootstrap fetch failed: {StatusCode} {ReasonPhrase}", 
                    response.StatusCode, response.ReasonPhrase);
                return null;
            }

            var bootstrap = await response.Content.ReadAsAsync<BootstrapResponse>(cancellationToken);
            
            _logger.Information("Bootstrap retrieved successfully: {CameraCount} cameras, {SensorCount} sensors",
                bootstrap?.Cameras?.Length ?? 0,
                bootstrap?.Sensors?.Length ?? 0);

            return bootstrap;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Bootstrap fetch error");
            return null;
        }
    }

    /// <summary>
    /// Check if currently connected and authenticated.
    /// </summary>
    public async Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                "/api/bootstrap",
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Configuration for UniFi Protect API client.
/// </summary>
public class ProtectApiConfiguration
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 443;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool VerifySsl { get; set; } = true;
    public int ConnectionTimeoutSeconds { get; set; } = 30;
    public int ReconnectIntervalSeconds { get; set; } = 30;
    public int HealthCheckIntervalSeconds { get; set; } = 60;
}

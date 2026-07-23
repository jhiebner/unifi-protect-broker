using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Serilog;

namespace ProtectBroker.Infrastructure.Services;

/// <summary>
/// WebSocket client for receiving real-time events from UniFi Protect.
/// Maintains persistent connection with automatic reconnection.
/// </summary>
public interface IProtectWebSocketClient
{
    event EventHandler<string>? OnEventReceived;
    event EventHandler? OnConnected;
    event EventHandler? OnDisconnected;
    
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync();
    Task<bool> IsConnectedAsync();
}

public class ProtectWebSocketClient : IProtectWebSocketClient, IDisposable
{
    private readonly ProtectApiConfiguration _config;
    private readonly ILogger _logger = Log.ForContext<ProtectWebSocketClient>();
    
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private DateTime _lastConnectionAttempt = DateTime.MinValue;
    private int _reconnectDelaySeconds = 5;
    private const int MaxReconnectDelaySeconds = 120;

    public event EventHandler<string>? OnEventReceived;
    public event EventHandler? OnConnected;
    public event EventHandler? OnDisconnected;

    public ProtectWebSocketClient(ProtectApiConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Start the WebSocket connection with automatic reconnection.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            _logger.Information("Starting UniFi Protect WebSocket client");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ConnectAsync(_cancellationTokenSource.Token);
                    _reconnectDelaySeconds = 5; // Reset delay on successful connection
                }
                catch (OperationCanceledException)
                {
                    _logger.Information("WebSocket operation cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "WebSocket connection failed, will retry in {DelaySeconds}s", 
                        _reconnectDelaySeconds);
                    
                    // Wait before reconnecting
                    await Task.Delay(
                        TimeSpan.FromSeconds(_reconnectDelaySeconds), 
                        _cancellationTokenSource.Token);
                    
                    // Exponential backoff for reconnection
                    _reconnectDelaySeconds = Math.Min(
                        _reconnectDelaySeconds * 2, 
                        MaxReconnectDelaySeconds);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error in WebSocket client");
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Connect to UniFi Protect WebSocket and listen for events.
    /// </summary>
    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        _webSocket = new ClientWebSocket();

        // Configure WebSocket
        _webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

        // Build WebSocket URI
        var wsUri = $"wss://{_config.Host}:{_config.Port}/api/ws";

        _logger.Information("Connecting to WebSocket: {Uri}", wsUri);

        try
        {
            await _webSocket.ConnectAsync(new Uri(wsUri), cancellationToken);
            _logger.Information("WebSocket connected successfully");
            OnConnected?.Invoke(this, EventArgs.Empty);

            // Listen for messages
            await ListenForMessagesAsync(cancellationToken);
        }
        finally
        {
            _webSocket?.Dispose();
            _webSocket = null;
            OnDisconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Listen for incoming messages from WebSocket.
    /// </summary>
    private async Task ListenForMessagesAsync(CancellationToken cancellationToken)
    {
        if (_webSocket == null) return;

        var buffer = new byte[1024 * 64];

        try
        {
            while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.Information("WebSocket close message received");
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing",
                        CancellationToken.None);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.Debug("WebSocket message received: {Message}", message);
                    OnEventReceived?.Invoke(this, message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Information("WebSocket listen operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error receiving WebSocket messages");
            throw;
        }
    }

    /// <summary>
    /// Stop the WebSocket connection.
    /// </summary>
    public async Task StopAsync()
    {
        _logger.Information("Stopping WebSocket client");
        
        _cancellationTokenSource?.Cancel();
        
        if (_webSocket?.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Stopping",
                    TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Error closing WebSocket");
            }
        }

        _webSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
    }

    /// <summary>
    /// Check if WebSocket is currently connected.
    /// </summary>
    public async Task<bool> IsConnectedAsync()
    {
        return _webSocket?.State == WebSocketState.Open;
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
        _webSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}

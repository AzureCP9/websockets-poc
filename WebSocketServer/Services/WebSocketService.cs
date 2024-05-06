using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using WebSocketServer.Domain.Entities;
using WebSocketServer.Domain.Factories;
using WebSocketServer.Dtos;
using WebSocketServer.Helpers;
using WebSocketServer.Services.Extensions;

namespace WebSocketServer.Services;
public class WebSocketService
{
    private readonly ILogger<WebSocketService> _logger;
    private readonly WebSocketMessageHandler _messageHandler;
    private readonly ChatCommandHandler _chatCommandHandler;
    private readonly WebSocketRoomService _roomService;
    private readonly int _messageBufferSize = 1024;
    private readonly ConcurrentDictionary<Guid, WebSocketConnection> _connectedClients = new();

    public WebSocketService(ILogger<WebSocketService> logger)
    {
        _logger = logger;
        _messageHandler = new WebSocketMessageHandler();
        _roomService = new WebSocketRoomService(_connectedClients);
        _chatCommandHandler = new ChatCommandHandler(_messageHandler, _roomService);
    }

    public async Task HandleRequest(HttpListenerContext context)
    {
        var webSocketConnectionResult = await context.GetWebSocketConnection();
        var webSocketConnection = webSocketConnectionResult.Connection;

        if (webSocketConnection == null)
        {
            _logger.LogInformation($"Connection closed for client '{context.Request.RemoteEndPoint.Address.MapToIPv4()}:{context.Request.RemoteEndPoint.Port}'. Reason: {webSocketConnectionResult.ErrorMessage}");

            await webSocketConnectionResult.WebSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, webSocketConnectionResult.ErrorMessage, CancellationToken.None);
            return;
        }

        var webSocket = webSocketConnection.WebSocket;
        AddConnection(webSocketConnection);
        await SendUserConnectedMessageAsync(webSocketConnection);
        
        try
        {
            while (webSocket.State is WebSocketState.Open)
            {
                var buffer = new Memory<byte>(new byte[_messageBufferSize]);

                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                var task = result.MessageType switch
                {
                    WebSocketMessageType.Text => HandleMessage(webSocketConnection, buffer),
                    _ => new ValueTask(webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None))
                };

                await task;
            }
        }
        catch (InvalidOperationException)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid payload provided. Please ensure a valid URI.", CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error handling websocket: {}", ex);
            var messageOutbound = MessageFactory.CreateErrorOutboundMessage(null, "Internal server error!");

            var errorDtoJson = JsonHelper.Serialize(messageOutbound);

            await _messageHandler.HandleMessageAsync(webSocket, errorDtoJson);


            //await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Internal server error!", CancellationToken.None);
        }
        finally
        {
            await EnsureClosedConnectionAndRemoveUser(webSocketConnection);
        }
    }

    private async ValueTask SendUserConnectedMessageAsync(WebSocketConnection connection)
    {
        var messageOutbound = MessageFactory.CreateUserConnectedOutboundMessage(connection.User.Id.Value.ToString());

        var userConnectedMessageJson = JsonHelper.Serialize(messageOutbound);

        await _messageHandler.HandleMessageAsync(connection.WebSocket, userConnectedMessageJson);
    }

    private async ValueTask HandleMessage(WebSocketConnection connection, Memory<byte> buffer)
    {
        try
        {
            var bufferStr = buffer.ToUTF8String();

            var messageInboundDto = JsonHelper.Deserialize<MessageInboundDto>(bufferStr);

            if (messageInboundDto is null) throw new ArgumentException("Invalid message payload");

            var command = messageInboundDto.Payload.Message.ToCommand();

            var userRoom = _roomService.GetUserCurrentRoom(connection.User.Id.Value);
            var recipients = _roomService.GetRecipientSocketsInUserRoom(userRoom);

            var webSocketMessage = new WebSocketMessage(command, recipients, messageInboundDto, userRoom);

            await _chatCommandHandler.ExecuteCommand(connection, webSocketMessage);
        } 
        catch (JsonException ex)
        {
            var messageOutbound = MessageFactory.CreateErrorOutboundMessage(null, ex.Message);

            var invalidMessageResponseString = JsonHelper.Serialize(messageOutbound);

            await _messageHandler.HandleMessageAsync(connection.WebSocket, invalidMessageResponseString);
        }
    }

    private async Task EnsureClosedConnectionAndRemoveUser(WebSocketConnection webSocketConnection)
    {
        var userId = webSocketConnection.User.Id.Value;
        var webSocket = webSocketConnection.WebSocket;

        if (webSocket.State is not WebSocketState.Closed)
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing...", CancellationToken.None);

        RemoveConnection(userId);

        _roomService.RemoveUserFromAllRooms(userId);
    }

    public void AddConnection(WebSocketConnection connection)
    {
        _connectedClients.TryAdd(connection.User.Id.Value, connection);
    }

    public void RemoveConnection(Guid userId)
    {
        _connectedClients.TryRemove(userId, out _);
    }
}

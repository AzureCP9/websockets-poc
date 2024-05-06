using WebSocketServer.Domain.Entities;
using WebSocketServer.Domain.Factories;
using WebSocketServer.Dtos;
using WebSocketServer.Helpers;
using WebSocketServer.Services.Extensions;

namespace WebSocketServer.Services;
public class ChatCommandHandler
{
    private readonly WebSocketMessageHandler _messageHandler;
    private readonly WebSocketRoomService _roomService;

    public ChatCommandHandler(WebSocketMessageHandler messageHandler, WebSocketRoomService roomService)
    {
        _messageHandler = messageHandler;
        _roomService = roomService;

    }

    public async ValueTask ExecuteCommand(WebSocketConnection userConnection, WebSocketMessage webSocketMessage)
    {
        var valueTask = (webSocketMessage.Command, webSocketMessage.Room) switch
        {
            (Command.JoinRoom, _) => JoinRoom(userConnection, webSocketMessage),
            (Command.LeaveRoom, _) => LeaveRoom(userConnection, webSocketMessage),
            (_, null) => NotInARoom(userConnection, webSocketMessage),
            (_, _) => SendMessage(userConnection, webSocketMessage),
        };

        await valueTask;
    }


    private async ValueTask SendMessage(WebSocketConnection userConnection, WebSocketMessage webSocketMessage)
    {
        var room = webSocketMessage.Room ?? throw new ArgumentNullException(nameof(webSocketMessage.Room));

        var messageOutboundUserMessage = new MessageOutboundUserMessage(
            roomId: room.Id.Value,
            userId: userConnection.User.Id.Value,
            userName: userConnection.User.Name.Value,
            message: webSocketMessage.Message.Payload.Message);

        var messageOutbound = MessageFactory.CreateNormalOutboundMessage(webSocketMessage.Message.FrontendId, messageOutboundUserMessage);

        var serializedData = JsonHelper.Serialize(messageOutbound);

        await _messageHandler.HandleMessageAsync(webSocketMessage.Recipients, serializedData);
    }

    private async ValueTask SendErrorMessage(WebSocketConnection userConnection, WebSocketMessage webSocketMessage, string message)
    {
        var messageOutbound = MessageFactory.CreateErrorOutboundMessage(webSocketMessage.Message.FrontendId, message);

        var serializedData = JsonHelper.Serialize(messageOutbound);

        await _messageHandler.HandleMessageAsync(userConnection.WebSocket, serializedData);
    }

    private async ValueTask SendNotification(WebSocketConnection userConnection, WebSocketMessage webSocketMessage, string message)
    {
        var messageOutbound = MessageFactory.CreateNotificationOutboundMessage(webSocketMessage.Message.FrontendId, message);

        var serializedData = JsonHelper.Serialize(messageOutbound);

        await _messageHandler.HandleMessageAsync(userConnection.WebSocket, serializedData);
    }

    private async ValueTask NotInARoom(WebSocketConnection userConnection, WebSocketMessage webSocketMessage)
    {
        await SendErrorMessage(userConnection, webSocketMessage, "You are not in any room.");
    }

    private async ValueTask RoomDoesNotExist(WebSocketConnection userConnection, WebSocketMessage webSocketMessage, string roomName)
    {
        await SendErrorMessage(userConnection, webSocketMessage, $"Room '{roomName} does not exist!'");
    }
    private async ValueTask JoinRoom(WebSocketConnection userConnection, WebSocketMessage message)
    {
        var roomName = message.Message.Payload.Message.ParseCommandArgs();

        var userAdded = _roomService.AddUserToRoomByRoomName(userConnection.User.Id.Value, roomName);

        if (!userAdded)
        {
            await RoomDoesNotExist(userConnection, message, roomName);
            return;
        }

        await SendNotification(userConnection, message, $"Joined room '{roomName}'.");
    }

    private async ValueTask LeaveRoom(WebSocketConnection userConnection, WebSocketMessage webSocketMessage)
    {
        var roomName = webSocketMessage.Room?.Name.Value;

        if (!string.IsNullOrEmpty(roomName))
        {
            await SendErrorMessage(userConnection, webSocketMessage, "The command /leaveroom takes no arguments.");
            return;
        }

        var userRemoved = _roomService.RemoveUserFromAllRooms(userConnection.User.Id.Value);

        var task = userRemoved switch
        {
            true => SendNotification(userConnection, webSocketMessage, "Left room."),
            false => NotInARoom(userConnection, webSocketMessage),
        };

        await task;
    }
}

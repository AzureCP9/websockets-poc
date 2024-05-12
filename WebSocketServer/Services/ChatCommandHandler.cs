using System.Net.WebSockets;
using WebSocketServer.Domain.Connection;
using WebSocketServer.Domain.Messaging;
using WebSocketServer.Domain.Messaging.Factories;
using WebSocketServer.Domain.Rooms.Failures;
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
            (Command.JoinRoom, _) => JoinRoomAsync(userConnection, webSocketMessage),
            (Command.LeaveRoom, _) => LeaveRoomAsync(userConnection, webSocketMessage),
            (_, null) => SendNotInARoomErrorAsync(userConnection, webSocketMessage),
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


    private async ValueTask SendErrorMessageAsync(WebSocketConnection userConnection, WebSocketMessage webSocketMessage, string message)
    {
        var messageOutbound = MessageFactory.CreateErrorOutboundMessage(webSocketMessage.Message.FrontendId, message);

        var serializedData = JsonHelper.Serialize(messageOutbound);

        await _messageHandler.HandleMessageAsync(userConnection.WebSocket, serializedData);
    }
    private async ValueTask NotifyUsersAsync(IEnumerable<WebSocket> recipients, string message)
    {
        var messageOutbound = MessageFactory.CreateNotificationOutboundMessage(null, message);

        var serializedData = JsonHelper.Serialize(messageOutbound);

        await _messageHandler.HandleMessageAsync(recipients, serializedData);
    }
    private async ValueTask NotifyUserAsync(WebSocketConnection userConnection, WebSocketMessage webSocketMessage, string message)
    {
        var messageOutbound = MessageFactory.CreateNotificationOutboundMessage(webSocketMessage.Message.FrontendId, message);

        var serializedData = JsonHelper.Serialize(messageOutbound);

        await _messageHandler.HandleMessageAsync(userConnection.WebSocket, serializedData);
    }

    private async ValueTask SendNotInARoomErrorAsync(WebSocketConnection userConnection, WebSocketMessage webSocketMessage)
    {
        await SendErrorMessageAsync(userConnection, webSocketMessage, "You are not in any room.");
    }

    private async ValueTask SendRoomDoesNotExistErrorAsync(WebSocketConnection userConnection, WebSocketMessage webSocketMessage, string roomName)
    {
        await SendErrorMessageAsync(userConnection, webSocketMessage, $"Room '{roomName}' does not exist!'");
    }

    private async ValueTask SendAlreadyInRoomErrorAsync(WebSocketConnection userConnection, WebSocketMessage webSocketMessage)
    {
        await SendErrorMessageAsync(userConnection, webSocketMessage, $"You are already in that room.");
    }
    private async ValueTask JoinRoomAsync(WebSocketConnection userConnection, WebSocketMessage webSocketMessage)
    {
        var roomName = webSocketMessage.Message.Payload.Message.ParseCommandArgs();
        var userAddedResult = _roomService.AddUserToRoomByRoomName(userConnection.User.Id.Value, roomName);

        if (userAddedResult.Failures.Any(f => f is RoomDoesNotExistFailure))
        {
            await SendRoomDoesNotExistErrorAsync(userConnection, webSocketMessage, roomName);
            return;
        }

        if (userAddedResult.Failures.Any(f => f is AlreadyInRoomFailure))
        {
            await SendAlreadyInRoomErrorAsync(userConnection, webSocketMessage);
            return;
        }

        var joinedRoom = _roomService.GetUserCurrentRoom(userConnection.User.Id.Value);
        var usersInRoom = _roomService.GetRecipientSocketsInRoomExceptUser(joinedRoom, userConnection.User.Id.Value);

        await SendUserJoinedNotificationAsync(userConnection, webSocketMessage, usersInRoom, roomName);
    }

    private async ValueTask SendUserJoinedNotificationAsync(WebSocketConnection userConnection, WebSocketMessage webSocketMessage, IEnumerable<WebSocket> usersInRoom, string roomName)
    {
        await NotifyUserAsync(userConnection, webSocketMessage, $"Joined room '{roomName}'.");
        await NotifyUsersAsync(usersInRoom, $"User '{userConnection.User.Name.Value}' joined the room.");
    }

    private async ValueTask LeaveRoomAsync(WebSocketConnection userConnection, WebSocketMessage webSocketMessage)
    {
        var commandArgs = webSocketMessage.Message.Payload.Message.ParseCommandArgs();

        if (!string.IsNullOrEmpty(commandArgs))
        {
            await SendErrorMessageAsync(userConnection, webSocketMessage, "The command /leaveroom takes no arguments.");
            return;
        }

        var userRemoved = _roomService.RemoveUserFromAllRooms(userConnection.User.Id.Value);

        ValueTask task = userRemoved switch
        {
            true => SendUserLeftRoomNotificationAsync(userConnection, webSocketMessage),
            false => SendNotInARoomErrorAsync(userConnection, webSocketMessage),
        };

        await task;
    }

    private async ValueTask SendUserLeftRoomNotificationAsync(WebSocketConnection userConnection, WebSocketMessage webSocketMessage)
    {
        await NotifyUserAsync(userConnection, webSocketMessage, $"Left room '{webSocketMessage.Room?.Name.Value}'.");
        await NotifyUsersAsync(webSocketMessage.Recipients, $"User '{userConnection.User.Name.Value}' left the room.");
    }
}

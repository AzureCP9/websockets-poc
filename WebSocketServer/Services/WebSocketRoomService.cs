using System.Collections.Concurrent;
using System.Net.WebSockets;
using WebSocketServer.Common;
using WebSocketServer.Domain.Connection;
using WebSocketServer.Domain.Rooms;
using WebSocketServer.Domain.Rooms.Failures;

namespace WebSocketServer.Services;
public class WebSocketRoomService
{
    private readonly ConcurrentDictionary<Guid, Room> _rooms = new();
    private readonly ConcurrentDictionary<Guid, WebSocketConnection> _connectedClients;

    public WebSocketRoomService(ConcurrentDictionary<Guid, WebSocketConnection> connectedClients)
    {
        _connectedClients = connectedClients;

        var defaultRoom = new Room(Guid.NewGuid(), "general");
        _rooms.TryAdd(defaultRoom.Id.Value, defaultRoom);
    }

    public bool RemoveUserFromAllRooms(Guid userId)
    {
        return _rooms.Values.Any(x => x.RemoveParticipant(userId));
    }

    public IEnumerable<WebSocket> GetRecipientSocketsInRoom(Room? room)
    {
        if (room == null)
            return Enumerable.Empty<WebSocket>();

        return _connectedClients
            .Where(x => room.ContainsParticipant(x.Key))
            .Select(x => x.Value.WebSocket);
    }

    public IEnumerable<WebSocket> GetRecipientSocketsInRoomExceptUser(Room? room, Guid userId)
    {
        if (room == null)
            return Enumerable.Empty<WebSocket>();

        return _connectedClients
            .Where(x => room.ContainsParticipant(x.Key) && x.Key != userId)
            .Select(x => x.Value.WebSocket);
    }

    public Room? GetUserCurrentRoom(Guid userId)
    {
        return _rooms
            .Where(x => x.Value.ContainsParticipant(userId))
            .FirstOrDefault().Value;
    }

    public Result AddUserToRoomByRoomName(Guid userId, string roomName)
    {
        var room = _rooms
            .Where(x => x.Value.Name.Value == roomName)
            .FirstOrDefault().Value;

        if (room == null)
            return Result.Failure(new RoomDoesNotExistFailure(roomName));

        if (room.ContainsParticipant(userId))
            return Result.Failure(new AlreadyInRoomFailure(userId));

        room.AddParticipant(userId);
        return Result.Success();
    }

    public Result RemoveUserFromRoomByRoomName(Guid userId, string roomName)
    {
        var room = _rooms
            .Where(x => x.Value.Name.Value == roomName)
            .FirstOrDefault().Value;

        if (room != null)
        {
            room.RemoveParticipant(userId);
            return Result.Success();
        }

        return Result.Failure();
    }
}

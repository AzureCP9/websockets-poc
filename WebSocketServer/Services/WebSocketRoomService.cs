using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using WebSocketServer.Domain.Aggregates.RoomEntities;
using WebSocketServer.Domain.Entities;

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

    public IEnumerable<WebSocket> GetRecipientSocketsInUserRoom(Room? room)
    {
        if (room == null)
            return Enumerable.Empty<WebSocket>();

        return _connectedClients
            .Where(x => room.ContainsParticipant(x.Key))
            .Select(x => x.Value.WebSocket);
    }

    public Room? GetUserCurrentRoom(Guid userId)
    {
        return _rooms
            .Where(x => x.Value.ContainsParticipant(userId))
            .FirstOrDefault().Value;
    }

    public bool AddUserToRoomByRoomName(Guid userId, string roomName)
    {
        var room = _rooms
            .Where(x => x.Value.Name.Value == roomName)
            .FirstOrDefault().Value;

        if (room != null)
        {
            room.AddParticipant(userId);
            return true;
        }

        return false;
    }

    public bool RemoveUserFromRoomByRoomName(Guid userId, string roomName)
    {
        var room = _rooms
            .Where(x => x.Value.Name.Value == roomName)
            .FirstOrDefault().Value;

        if (room != null)
        {
            room.RemoveParticipant(userId);
            return true;
        }

        return false;
    }
}

using System.Net.WebSockets;
using WebSocketServer.Domain.Aggregates.RoomEntities;
using WebSocketServer.Dtos;

namespace WebSocketServer.Domain.Entities;
public class WebSocketMessage
{
    public Command Command { get; init; }
    public IEnumerable<WebSocket> Recipients { get; init; }
    public Room? Room { get; init; }
    public MessageInboundDto Message { get; init; }
    public WebSocketMessage(Command command, IEnumerable<WebSocket> recipients, MessageInboundDto message, Room? room)
    {
        Command = command;
        Recipients = recipients;
        Message = message;
        Room = room;
    }
}

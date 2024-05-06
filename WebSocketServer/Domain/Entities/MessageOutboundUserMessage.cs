namespace WebSocketServer.Domain.Entities;
public record MessageOutboundUserMessage
{
    public Guid RoomId { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; }
    public string Message { get; init; }

    public MessageOutboundUserMessage(Guid roomId, Guid userId, string userName, string message)
    {
        RoomId = roomId;
        UserId = userId;
        UserName = userName;
        Message = message;
    }
}

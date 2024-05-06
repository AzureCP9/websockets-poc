namespace WebSocketServer.Domain.Entities;
public record MessageOutbound
{
    public MessageType Type { get; init; }
    public Guid? FrontendId { get; init; }
    public Guid BackendId { get; } = Guid.NewGuid();
    public string ServerMessage { get; init; }
    public DateTime TimeStamp { get; init; }
    public MessageOutboundUserMessage? UserMessage { get; init; }
    public MessageOutbound(MessageType type, Guid? frontendId, string serverMessage, MessageOutboundUserMessage? userMessage = null)
    {
        Type = type;
        FrontendId = frontendId;
        ServerMessage = serverMessage;
        TimeStamp = DateTime.UtcNow;
        UserMessage = userMessage;
    }
}

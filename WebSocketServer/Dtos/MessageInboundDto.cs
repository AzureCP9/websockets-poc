using WebSocketServer.Domain.Messaging;

namespace WebSocketServer.Dtos;
public class MessageInboundDto
{
    public MessageType Type { get; } = MessageType.Message;
    public required Guid FrontendId { get; set; }
    public required MessageInboundPayloadDto Payload { get; set; }
}

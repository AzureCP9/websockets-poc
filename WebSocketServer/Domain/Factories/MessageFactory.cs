using WebSocketServer.Domain.Entities;

namespace WebSocketServer.Domain.Factories;
public static class MessageFactory
{
    public static MessageOutbound CreateNormalOutboundMessage(Guid frontendId, MessageOutboundUserMessage? userMessage)
    {
        return new MessageOutbound(MessageType.Message, frontendId, "User message", userMessage);
    }

    public static MessageOutbound CreateErrorOutboundMessage(Guid? frontendId, string errorMessage, MessageOutboundUserMessage? userMessage = null)
    {
        return new MessageOutbound(MessageType.Error, frontendId, errorMessage, userMessage);
    }
    public static MessageOutbound CreateNotificationOutboundMessage(Guid frontendId, string notificationMessage, MessageOutboundUserMessage? userMessage = null)
    {
        return new MessageOutbound(MessageType.Notification, frontendId, notificationMessage, userMessage);
    }
    public static MessageOutbound CreateUserConnectedOutboundMessage(string userId)
    {
        return new MessageOutbound(MessageType.UserConnected, frontendId: null, userId);
    }
}

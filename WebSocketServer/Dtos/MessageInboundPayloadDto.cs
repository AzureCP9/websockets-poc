﻿namespace WebSocketServer.Dtos;
public class MessageInboundPayloadDto
{
    public string? RoomId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

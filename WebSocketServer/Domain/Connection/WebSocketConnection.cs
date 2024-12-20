﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using WebSocketServer.Domain.Users;

namespace WebSocketServer.Domain.Connection;
public class WebSocketConnection
{
    public User User { get; init; }

    public WebSocket WebSocket { get; init; }

    public WebSocketConnection(Guid userId, string userName, WebSocket webSocket)
    {
        User = new(userId, userName);
        WebSocket = webSocket;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketServer.Domain.Connection;
public record WebSocketConnectionResult(WebSocketConnection? Connection, WebSocket WebSocket, bool IsSuccessful, string? ErrorMessage);

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebSocketServer.Domain.Entities;

namespace WebSocketServer.Services.Extensions;
public static class HttpListenerContextExtensions
{
    public static async Task<WebSocketConnectionResult> GetWebSocketConnection(this HttpListenerContext self)
    {
        var webSocketContext = await self.AcceptWebSocketAsync(subProtocol: null);

        try
        {
            var webSocket = webSocketContext.WebSocket;
            var userId = Guid.NewGuid();
            var queryParams = HttpUtility.ParseQueryString(self.Request.Url?.Query ?? throw new InvalidOperationException("Invalid URI"));
            var userName = queryParams["username"] ?? throw new InvalidOperationException("Invalid URI");

            return new WebSocketConnectionResult(new WebSocketConnection(userId, userName, webSocket), webSocket, true, null);
        }
        catch (InvalidOperationException ex)
        {
            return new WebSocketConnectionResult(null, webSocketContext.WebSocket, false, ex.Message);
        }
        
    }

    public static void HandleBadRequest(this HttpListenerContext self, string? statusDescription)
    {
        if (!string.IsNullOrEmpty(statusDescription))
        {
            byte[] buffer = Encoding.UTF8.GetBytes(statusDescription);
            self.Response.ContentLength64 = buffer.Length;
            self.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        self.Response.StatusCode = 400;
        self.Response.Close();
    }
}

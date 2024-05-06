using System.Net.WebSockets;
using WebSocketServer.Services.Extensions;

namespace WebSocketServer.Services;
public class WebSocketMessageHandler
{
    public async ValueTask HandleMessageAsync(IEnumerable<WebSocket> webSockets, string data)
    {
        var buffer = data.ToMemoryOfByte();

        await webSockets
            .Select(x => x.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None))
            .WhenAll();
    }

    public async ValueTask HandleMessageAsync(WebSocket webSocket, string data)
    {
        var buffer = data.ToMemoryOfByte();

        await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}

using Microsoft.Extensions.Logging;
using System.Net;
using WebSocketServer.Services;
using WebSocketServer.Services.Extensions;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole();
});

var logger = loggerFactory.CreateLogger<WebSocketService>();

var httpListener = new HttpListener();
httpListener.Prefixes.Add("http://localhost:5000/");
httpListener.Start();

Console.WriteLine("Listening...");
var webSocketHandler = new WebSocketService(logger);

while (true)
{
    var context = await httpListener.GetContextAsync();
    Console.WriteLine($"Client connected: {context.Request.RemoteEndPoint.Address.MapToIPv4()}:{context.Request.RemoteEndPoint.Port}");
    if (context.Request.IsWebSocketRequest)
        _ = Task.Run(() => webSocketHandler.HandleRequest(context));
    else
        context.HandleBadRequest("Only WebSocket connections are allowed on this server.");
}

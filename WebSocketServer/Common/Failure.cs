namespace WebSocketServer.Common;
public abstract record Failure
{
    public Failure(string message) =>
        Message = message;

    public string Message { get; init; }
}

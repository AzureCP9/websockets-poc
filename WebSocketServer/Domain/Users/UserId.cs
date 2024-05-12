namespace WebSocketServer.Domain.Users;

public class UserId
{
    public Guid Value { get; init; }

    public UserId(Guid value)
    {
        Value = value;
    }
}
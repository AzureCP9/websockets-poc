namespace WebSocketServer.Domain.Aggregates.UserEntities;

public class UserId
{
    public Guid Value { get; init; }

    public UserId(Guid value)
    {
        Value = value;
    }
}
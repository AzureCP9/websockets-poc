using WebSocketServer.Domain.ValueTypes;

namespace WebSocketServer.Domain.Aggregates.RoomEntities;
public class Room
{
    public RoomId Id { get; init; }

    public Name Name { get; init; }

    private readonly HashSet<Guid> _participantIds = new();

    public Room(Guid id, string name)
    {
        Id = new(id);
        Name = new(name);
    }

    public void AddParticipant(Guid id)
    {
        _participantIds.Add(id);
    }

    public bool RemoveParticipant(Guid userId)
    {
        return _participantIds.Remove(userId);
    }

    public bool ContainsParticipant(Guid userId)
    {
        return _participantIds.Contains(userId);
    }
    public IEnumerable<Guid> GetParticipants() => _participantIds.AsEnumerable();
}

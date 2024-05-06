using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketServer.Domain.Aggregates.RoomEntities;
using WebSocketServer.Domain.ValueTypes;

namespace WebSocketServer.Domain.Aggregates.UserEntities;
public class User
{
    public UserId Id { get; init; }

    public Name Name { get; init; }

    public User(Guid id, string name)
    {
        Id = new(id);
        Name = new(name);
    }
}

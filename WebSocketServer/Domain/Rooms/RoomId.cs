using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketServer.Domain.Rooms;
public class RoomId
{
    public Guid Value { get; init; }

    public RoomId(Guid value)
    {
        Value = value;
    }
}

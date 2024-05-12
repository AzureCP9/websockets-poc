using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketServer.Common;

namespace WebSocketServer.Domain.Rooms.Failures;
public record RoomDoesNotExistFailure : Failure
{
    public RoomDoesNotExistFailure(string roomName) : base($"Room '{roomName}' does not exist.")
    {

    }
}

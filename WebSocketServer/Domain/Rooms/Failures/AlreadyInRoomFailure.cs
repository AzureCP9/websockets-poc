using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketServer.Common;

namespace WebSocketServer.Domain.Rooms.Failures;
public record AlreadyInRoomFailure : Failure
{
  
    public AlreadyInRoomFailure(Guid userId) : base($"User with id '{userId}' is already in room.")
    {

    }

}

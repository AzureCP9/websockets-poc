using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketServer.Domain.Messaging;
public enum Command
{
    [Description("/joinroom")]
    JoinRoom,
    [Description("/leaveroom")]
    LeaveRoom,
    Message
}

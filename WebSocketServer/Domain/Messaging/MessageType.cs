using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketServer.Domain.Messaging;
public enum MessageType
{
    UserConnected,
    Message,
    Notification,
    Error,
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketServer.Domain.Messaging;
public class MessageId
{
    public Guid Value { get; init; }

    public MessageId()
    {
        Value = Guid.NewGuid();
    }
}

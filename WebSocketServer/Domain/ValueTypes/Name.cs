using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketServer.Domain.ValueTypes;
public class Name
{
    public string Value { get; init; }

    public Name(string value)
    {
        Value = value;
    }
}

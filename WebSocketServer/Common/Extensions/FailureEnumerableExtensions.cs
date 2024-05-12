using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketServer.Common.Extensions;
public static class FailureEnumerableExtensions
{
    public static string ToPrettyMessage(this IEnumerable<Failure> self)
    {
        return $"{string.Join(". ", self)}";
    }
}

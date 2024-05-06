using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketServer.Services.Extensions;
public static class ValueTaskEnumerableExtensions
{
    public static async ValueTask WhenAll(this IEnumerable<ValueTask> self)
    {
        foreach (var valueTask in self)
        {
            if (!valueTask.IsCompleted)
            {
                await valueTask;
            }
        }
    }
}
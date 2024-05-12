using System.Text;

namespace WebSocketServer.Services.Extensions;
public static class MemoryOfByteExtensions
{

    public static string ToUTF8String(this Memory<byte> self)
    {
        var span = self.Span;

        var stringEnd = span.IndexOf((byte)0);

        if (stringEnd == -1)
        {
            stringEnd = span.Length;
        }

        return Encoding.UTF8.GetString(span.Slice(0, stringEnd));
    }
}

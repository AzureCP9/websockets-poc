using System.Text;
using WebSocketServer.Domain.Messaging;
using WebSocketServer.Helpers;

namespace WebSocketServer.Services.Extensions;
public static class StringExtensions
{
    public static Memory<byte> ToMemoryOfByte(this string self)
    {
        return Encoding.UTF8.GetBytes(self);
    }

    public static Command ToCommand(this string self)
    {
        var stringCommand = self.ParseCommand();

        var isValid = EnumHelper.TryParseFromDescription(stringCommand, out Command result);

        if (isValid)
            return result;

        return Command.Message;
    }

    public static string ParseCommandArgs(this string self)
    {
        var parts = self
            .Split(new[] { ' ' }, 2);
        if (parts.Length > 1)
        {
            return parts[1];
        }

        return string.Empty;
    }

    public static string ParseCommand(this string self)
    {
        var parts = self
            .Split(new[] { ' ' }, 2);

        return parts[0];
    }
}

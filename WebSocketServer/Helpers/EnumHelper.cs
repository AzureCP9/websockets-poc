using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketServer.Helpers;
public static class EnumHelper
{
    public static bool TryParseFromDescription<T>(string value, out T? result) where T : Enum
    {
        foreach (var field in typeof(T).GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                if (attribute.Description == value.ToLower())
                {
                    result = (T)field.GetValue(null)!; // can safely assert non null as Enum members are static and cannot be null if description is found
                    return true;
                }
            }
        }

        result = default;
        return false;
    }
}

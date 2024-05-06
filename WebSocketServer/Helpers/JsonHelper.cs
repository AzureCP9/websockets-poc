using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace WebSocketServer.Helpers;
public static class JsonHelper
{
    public static string Serialize<T>(T value)
    {
        var settings = new JsonSerializerSettings()
        {
            Converters = new[] { new StringEnumConverter() },
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        return JsonConvert.SerializeObject(value, settings);
    }

    public static T? Deserialize<T>(string json)
    {
        var settings = new JsonSerializerSettings()
        {
            Converters = new[] { new StringEnumConverter() },
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        return JsonConvert.DeserializeObject<T>(json, settings);
    }
}

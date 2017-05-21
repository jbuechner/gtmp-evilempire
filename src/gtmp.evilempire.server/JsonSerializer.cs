using gtmp.evilempire.services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace gtmp.evilempire.server
{
    class JsonSerializer : IJsonSerializer
    {
        JsonSerializerSettings Settings { get; }

        public JsonSerializer()
        {
            Settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        }

        public string Stringify(object value)
        {
            if (value == null)
            {
                return "{}";
            }

            var type = value.GetType();
            return JsonConvert.SerializeObject(value, type, Formatting.None, Settings);
        }

        public dynamic Parse(string json)
        {
            return JObject.Parse(json);
        }
    }
}

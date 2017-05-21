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

        public string Stringify(object o)
        {
            if (o == null)
            {
                return "{}";
            }

            var type = o.GetType();
            return JsonConvert.SerializeObject(o, type, Formatting.None, Settings);
        }

        public dynamic Parse(string json)
        {
            return JObject.Parse(json);
        }
    }
}

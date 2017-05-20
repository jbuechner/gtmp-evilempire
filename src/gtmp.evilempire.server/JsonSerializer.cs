using gtmp.evilempire.services;
using System.IO;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gtmp.evilempire.server
{
    class JsonSerializer : IJsonSerializer
    {
        Newtonsoft.Json.JsonSerializer serializer;

        public JsonSerializer()
        {
            this.serializer = Newtonsoft.Json.JsonSerializer.Create();
        }

        public string Stringify(object o)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                this.serializer.Serialize(writer, o);
                return sb.ToString();
            }
        }

        public dynamic Parse(string json)
        {
            return JObject.Parse(json);
        }
    }
}

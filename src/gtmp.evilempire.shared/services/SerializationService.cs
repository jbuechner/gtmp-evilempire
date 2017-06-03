using Newtonsoft.Json;
using System;

namespace gtmp.evilempire.services
{
    public class SerializationService : ISerializationService
    {
        const string JsonDesignation = "$json";
        const string EmptyDesignatedJson = JsonDesignation + "{}";

        public object DeserializeFromDesignatedJson(string s)
        {
            if (s == null || s.IndexOf(JsonDesignation, StringComparison.Ordinal) != 0)
            {
                return s;
            }
            var v = s.Substring(JsonDesignation.Length);
            return JsonConvert.DeserializeObject(v);
        }

        public string SerializeAsDesignatedJson(object o)
        {
            if (o == null)
            {
                return EmptyDesignatedJson;
            }
            return DecorateAsJson(JsonConvert.SerializeObject(o));
        }

        public string DecorateAsJson(string value)
        {
            return string.Concat(JsonDesignation, value);
        }
    }
}

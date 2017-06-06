using gtmp.evilempire.server.mapping;
using gtmp.evilempire.services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.messages
{
    class EntityContentResponse
    {
        public int EntityId { get; set; }
        public string Content { get; set; }

        public EntityContentResponse(ISerializationService serialization, int entityId, MapDialoguePage page)
        {
            var content = SerializeDialoguePage(page);
            Content = serialization.DecorateAsJson(content);
            EntityId = entityId;
        }

        public EntityContentResponse(int entityId, string content)
        {
            EntityId = entityId;
            Content = content;
        }

        static string SerializeDialoguePage(MapDialoguePage page)
        {
            var builder = new StringBuilder();
            using (var textWriter = new StringWriter(builder))
            using (var writer = new JsonTextWriter(textWriter))
            {
                SerializeDialoguePage(page, writer, false);
                writer.Flush();
            }
            return builder.ToString();
        }

        static void SerializeDialoguePage(MapDialoguePage page, JsonTextWriter writer, bool writeNestedPages)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("key");
            writer.WriteValue(page.Key);
            if (!string.IsNullOrEmpty(page.Markdown))
            {
                writer.WritePropertyName("markdown");
                writer.WriteValue(page.Markdown);
            }
            writer.WriteEndObject();
        }
    }
}

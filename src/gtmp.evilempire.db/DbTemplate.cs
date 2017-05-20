using gtmp.evilempire.entities.processors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace gtmp.evilempire.db
{
    public class DbTemplate
    {
        string path;

        public DbTemplate(string databaseTemplateRootPath)
        {
            this.path = databaseTemplateRootPath;
        }

        public IEnumerable<string> GetTemplates()
        {
            if (!Directory.Exists(path))
            {
                yield break;
            }
            foreach(var file in Directory.GetFiles(path, "*.xml"))
            {
                yield return file;
            }
        }

        public bool PopulateByTemplate(string template, DbEnvironment dbEnvironment)
        {
            using (var file = File.Open(template, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var xmlReader = XmlReader.Create(file))
            {
                if (!xmlReader.ReadToDescendant("elements"))
                {
                    return false;
                }
                var fullyQualifiedName = xmlReader.GetAttribute("type");
                var t = Type.GetType(fullyQualifiedName);

                var customProcessorFullyQualifiedName = xmlReader.GetAttribute("customProcessor");
                IEntityProcessor entityProcessor = null;
                if (customProcessorFullyQualifiedName != null)
                {
                    var customProcessorType = Type.GetType(customProcessorFullyQualifiedName);
                    entityProcessor = Activator.CreateInstance(customProcessorType) as IEntityProcessor;
                }

                if (!xmlReader.ReadToDescendant(t.Name))
                {
                    return true;
                }

                XmlSerializer serializer = new XmlSerializer(t);
                do
                {
                    var o = serializer.Deserialize(xmlReader);
                    if (o != null)
                    {
                        var key = dbEnvironment.SelectKey(o);
                        if (entityProcessor != null)
                        {
                            entityProcessor.Process(o);
                        }
                        dbEnvironment.InsertOrUpdate(key, o);
                    }

                } while (xmlReader.ReadToNextSibling(t.Name));

            }
            return true;
        }
    }
}

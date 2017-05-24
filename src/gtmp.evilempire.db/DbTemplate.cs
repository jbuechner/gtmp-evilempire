using gtmp.evilempire.entities.processors;
using gtmp.evilempire.services;
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

        public IEnumerable<string> Templates
        {
            get
            {
                if (!Directory.Exists(path))
                {
                    yield break;
                }
                foreach (var file in Directory.GetFiles(path, "*.xml"))
                {
                    yield return file;
                }
            }
        }

        public DbTemplate(string databaseTemplateRootPath)
        {
            path = databaseTemplateRootPath;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static bool PopulateByTemplate(string template, IDbService dbEnvironment)
        {
            if (!File.Exists(template))
            {
                throw new FileNotFoundException(template);
            }
            if (dbEnvironment == null)
            {
                throw new ArgumentNullException(nameof(dbEnvironment));
            }

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
                        if (entityProcessor != null)
                        {
                            entityProcessor.Process(o);
                        }
                        dbEnvironment.InsertOrUpdate(o);
                    }

                } while (xmlReader.ReadToNextSibling(t.Name));

            }

            return true;
        }
    }
}

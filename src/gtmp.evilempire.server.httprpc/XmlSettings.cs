using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace gtmp.evilempire.server.httprpc
{
    static class XmlSettings
    {
        public static Settings ReadFrom(string fileName)
        {
            XDocument xdoc = XDocument.Load(fileName);
            var settingsElement = xdoc.Root?.Element("httprpc");
            var address = settingsElement?.Element("address")?.Value;
            var port = settingsElement?.Element("port")?.Value.AsInt() ?? 0;
            return new Settings
            {
                Address = address,
                Port = port
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.launcher
{
    static class Constants
    {
        public const string SettingsTemplateFile = "settings.template.xml";
        public const string SettingsUserFile = "settings.user.xml";
        public const string SettingsTransformationFile = "settings.xsl";
        public const string SettingFile = "settings.xml";
        public const string ServerExecutable = "GrandTheftMultiplayer.Server.exe";
        public const string HttpRpcServerExecutable = "gtmp.evilempire.server.httprpc.exe";

        internal static class ExitCodes
        {
            public const int ServerSettingsTransformationFailed = -100;
            public const int DatabaseCheckFailed = -200;
            public const int DatabasePopulationFailed = -201;
        }
    }
}

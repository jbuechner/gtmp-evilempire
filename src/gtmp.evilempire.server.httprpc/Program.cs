﻿namespace gtmp.evilempire.server.httprpc
{
    class Program
    {
        static void Main(string[] args)
        {
            var argumentParser = new ArgumentParser<Settings>()
                .AddHandler("address", (context, value) => context.Address = value)
                .AddHandler("port", (context, value) => context.Port = value.AsInt() ?? 0);
            var settings = XmlSettings.ReadFrom("settings.xml");

            argumentParser.Parse(settings, args);

            var httpRealm = new HttpListenerRealm(settings.Address, settings.Port);
        }
    }
}

// Decompiled with JetBrains decompiler
// Type: gtmp.evilempire.server.Main
// Assembly: gtmp.evilempire.server, Version=0.0.1.0, Culture=en-us, PublicKeyToken=null
// MVID: 33050A3E-AE8F-443D-9B50-16F7A45B55DE
// Assembly location: Z:\dev\git\gtav\gtmp\dist\debug\gtmp.evilempire.server.dll

using GrandTheftMultiplayer.Server.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gtmp.evilempire.server
{
  public class Main : Script
  {
    public Main()
    {
      this.API.onResourceStart += new GrandTheftMultiplayer.Server.API.API.EmptyEvent(this.OnResourceStart);
    }

    private void OnResourceStart()
    {
      this.API.onPlayerBeginConnect += (GrandTheftMultiplayer.Server.API.API.PlayerConnectingEvent) ((client, e) => Console.WriteLine(string.Format("{0:HH:mm:ss.fff} Player begin connect {1}, {2}, {3}", new object[4]
      {
        (object) DateTime.Now,
        (object) client.address,
        (object) client.name,
        (object) client.nametag
      })));
      this.API.onPlayerConnected += (GrandTheftMultiplayer.Server.API.API.PlayerEvent) (client => Console.WriteLine(string.Format("{0:HH:mm:ss.fff} Player connected {1}, {2}, {3}", new object[4]
      {
        (object) DateTime.Now,
        (object) client.address,
        (object) client.name,
        (object) client.nametag
      })));
      this.API.onPlayerDisconnected += (GrandTheftMultiplayer.Server.API.API.PlayerDisconnectedEvent) ((client, reason) => Console.WriteLine(string.Format("{0:HH:mm:ss.fff} Player disconnected {1}, {2}, {3}", new object[4]
      {
        (object) DateTime.Now,
        (object) client.address,
        (object) client.name,
        (object) client.nametag
      })));
      this.API.onPlayerFinishedDownload += (GrandTheftMultiplayer.Server.API.API.PlayerEvent) (client => Console.WriteLine(string.Format("{0:HH:mm:ss.fff} Player finsihed download {1}, {2}, {3}", new object[4]
      {
        (object) DateTime.Now,
        (object) client.address,
        (object) client.name,
        (object) client.nametag
      })));
      this.API.onClientEventTrigger += (GrandTheftMultiplayer.Server.API.API.ServerEventTrigger) ((client, name, arguments) => Console.WriteLine(string.Format("{0:HH:mm:ss.fff} Player trigger event {1}, {2}, {3} [ {4} {5} ]", (object) DateTime.Now, (object) client.address, (object) client.name, (object) client.nametag, (object) name, (object) string.Join(", ", ((IEnumerable<object>) arguments).Select<object, string>((Func<object, string>) (s => (s ?? (object) "").ToString()))))));
    }
  }
}

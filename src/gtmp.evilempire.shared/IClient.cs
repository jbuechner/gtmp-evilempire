using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire
{
    public interface IClient : IEquatable<IClient>
    {
        object PlatformObject { get; }

        bool IsNametagVisible { get; set; }
        int Dimension { get; set; }
        bool CanMove { get; set; }
        Vector3f Position { get; set; }
        Vector3f Rotation { get; set; }
        bool IsConnected { get; }

        string Name { get; }


        void StopAnimation();

        void TriggerClientEvent(string eventName, params object[] args);

        void SetData(string key, object value);
        object GetData(string key);

        void SendChatMessage(string message);

        // Additional non GTMP properties /methods
        string Login { get; set; }
        int CharacterId { get; set; }
        AuthUserGroup UserGroup { get; set; }
    }
}

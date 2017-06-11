using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.sessions
{
    public interface ISession
    {
        IClient Client { get; }
        SessionState State { get; set; }

        User User { get; set; }
        Character Character { get; set; }
        CharacterCustomization CharacterCustomization { get; set; }
        int PrivateDimension { get; set; }
        bool UpdateDatabasePosition { get; set; }
    }
}

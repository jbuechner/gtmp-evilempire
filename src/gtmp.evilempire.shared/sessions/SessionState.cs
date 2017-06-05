using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.sessions
{
    public enum SessionState
    {
        None = 0,
        Connected = 1,
        Ready = 2,
        LoggedIn = 3,
        CharacterCustomization = 4,
        Freeroam = 100
    }
}

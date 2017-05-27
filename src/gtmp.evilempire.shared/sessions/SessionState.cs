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
        LoggedIn = 2,
        CharacterCustomization = 3,
        Freeroam = 100
    }
}

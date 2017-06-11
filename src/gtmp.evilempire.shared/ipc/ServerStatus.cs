using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.ipc
{
    public struct ServerStatus
    {
        public string Version;
        public int MaximumNumbersOfPlayers;
        public int CurrentNumberOfPlayers;
    }
}

using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.messages.transfer
{
    class ClientUser
    {
        User user;

        public string Login
        {
            get
            {
                return user.Login;
            }
        }

        public AuthUserGroup UserGroup
        {
            get
            {
                return user.UserGroup;
            }
        }

        public ClientUser(User user)
        {
            this.user = user;
        }
    }
}

﻿using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.services
{
    public interface ILoginService
    {
        IServiceResult<User> Login(string login, IClient client);
    }
}
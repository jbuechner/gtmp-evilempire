using gtmp.evilempire.entities;
using gtmp.evilempire.services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.services
{
    class LoginService : ILoginService
    {
        IDbService DbService { get; }

        ConcurrentDictionary<string, IClient> LoggedInClients { get; } = new ConcurrentDictionary<string, IClient>();

        public LoginService(IDbService dbService)
        {
            DbService = dbService;
        }

        public IServiceResult<User> Login(string login, IClient client)
        {
            var user = DbService.SelectEntity<User, string>(login);
            if (user == null)
            {
                return ServiceResult<User>.AsError("Failed login");
            }

            IClient loggedInClient;
            if (LoggedInClients.TryGetValue(login, out loggedInClient))
            {
                if (!client.Equals(loggedInClient))
                {
                    return ServiceResult<User>.AsError("Failed login");
                }
            }
            else
            {
                LoggedInClients.AddOrUpdate(login, client, (key, value) => client);
            }

            return ServiceResult<User>.AsSuccess(user);
        }
    }
}

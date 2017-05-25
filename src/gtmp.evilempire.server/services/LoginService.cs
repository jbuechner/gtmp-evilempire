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
        ICharacterService CharacterService { get; }

        ConcurrentDictionary<string, IClient> LoggedInClients { get; } = new ConcurrentDictionary<string, IClient>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public LoginService(IDbService dbService, ICharacterService characterService)
        {
            DbService = dbService;
            CharacterService = characterService;
        }

        public IServiceResult<User> Login(string login, IClient client)
        {
            var user = DbService.Select<User, string>(login);
            if (user == null)
            {
                return ServiceResult<User>.AsError("Failed login");
            }

            IClient loggedInClient;
            if (LoggedInClients.TryGetValue(login, out loggedInClient))
            {
                if (!client.Equals(loggedInClient))
                {
                    user.NumberOfInvalidLoginAttempts += 1;
                    DbService.Update(user);
                    return ServiceResult<User>.AsError("Failed login");
                }
                else
                {
                    LoggedInClients.AddOrUpdate(login, client, (key, value) => client);
                }
            }
            else
            {
                LoggedInClients.AddOrUpdate(login, client, (key, value) => client);
            }

            if (user.FirstLogin == null)
            {
                user.FirstLogin = DateTime.Now;
            }

            client.Login = login;
            var activeCharacter = CharacterService.GetActiveCharacter(client);

            user.NumberOfInvalidLoginAttempts = 0;
            user.LastSuccessfulLogin = DateTime.Now;
            client.CharacterId = activeCharacter.Id;
            DbService.Update(user);

            return ServiceResult<User>.AsSuccess(user);
        }
    }
}

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

        public DateTime LastLoggedInClientsChangeTime { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public LoginService(IDbService dbService, ICharacterService characterService)
        {
            DbService = dbService;
            CharacterService = characterService;
            LastLoggedInClientsChangeTime = DateTime.MinValue;
        }

        public bool IsLoggedIn(IClient client)
        {
            if (client == null || client.Login == null)
            {
                return false;
            }

            return LoggedInClients.ContainsKey(client.Login);
        }

        public void Purge()
        {
            var changed = false;
            var keys = LoggedInClients.Keys.ToList();
            foreach(var key in keys)
            {
                IClient client;
                if (LoggedInClients.TryGetValue(key, out client))
                {
                    if (client == null || !client.IsConnected)
                    {
                        Logout(client);
                        changed = true;
                        continue;
                    }
                }
            }
            if (changed)
            {
                LastLoggedInClientsChangeTime = DateTime.Now;
            }
        }

        public User FindUserByLogin(string login)
        {
            var user = DbService.Select<User, string>(login);
            return user;
        }

        public IClient FindLoggedInClientByLogin(string login)
        {
            IClient loggedInClient;
            if (LoggedInClients.TryGetValue(login, out loggedInClient))
            {
                return loggedInClient;
            }
            return null;
        }

        public IServiceResult<User> Login(string login, IClient client)
        {
            var user = FindUserByLogin(login);
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
                    LastLoggedInClientsChangeTime = DateTime.Now;
                }
            }
            else
            {
                LoggedInClients.AddOrUpdate(login, client, (key, value) => client);
                LastLoggedInClientsChangeTime = DateTime.Now;
            }

            if (user.FirstLogin == null)
            {
                user.FirstLogin = DateTime.Now;
            }

            client.Login = login;
            client.UserGroup = user.UserGroup;
            var activeCharacter = CharacterService.GetActiveCharacter(client);

            user.NumberOfInvalidLoginAttempts = 0;
            user.LastSuccessfulLogin = DateTime.Now;
            client.CharacterId = activeCharacter.Id;
            DbService.Update(user);

            // create a copy of the user because the result is serialized and send back to the client
            var transferUser = new User { Login = user.Login, UserGroup = user.UserGroup };
            return ServiceResult<User>.AsSuccess(transferUser);
        }

        public void Logout(IClient client)
        {
            LoggedInClients.TryRemove(client.Login, out client);
        }

        public IList<IClient> GetLoggedInClients()
        {
            var list = new List<IClient>(LoggedInClients.Count);
            list.AddRange(LoggedInClients.Values);
            return list;
        }
    }
}

using gtmp.evilempire.entities;
using gtmp.evilempire.services;
using System;

namespace gtmp.evilempire.server.services
{
    class CharacterService : ICharacterService
    {
        IDbService DbService { get; }

        public CharacterService(IDbService dbService)
        {
            DbService = dbService;
        }

        public Character GetActiveCharacter(IClient client)
        {
            throw new NotImplementedException();
        }
    }
}

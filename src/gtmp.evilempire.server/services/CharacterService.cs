using gtmp.evilempire.entities;
using gtmp.evilempire.services;
using System;
using System.Linq;

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
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            if (client.Login == null)
            {
                throw new InvalidOperationException("Can not select active character for a client without an associated login.");
            }

            var character = DbService.SelectMany<Character, string>(client.Login)?.FirstOrDefault();

            if (character == null)
            {
                var characterId = DbService.NextValueFor(Constants.Database.Sequences.CharacterIdSequence);
                character = new Character { AssociatedLogin = client.Login, Id = characterId };
                DbService.Insert(character);
            }

            return character;
        }

        public void UpdatePosition(int characterId, Vector3f position)
        {
            var character = DbService.Select<Character, int>(ks => ks.Id, characterId);
            character.Position = position;
            DbService.Update<Character>(character);
        }
    }
}

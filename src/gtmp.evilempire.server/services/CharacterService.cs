using gtmp.evilempire.entities;
using gtmp.evilempire.services;
using gtmp.evilempire.sessions;
using System;
using System.Linq;

namespace gtmp.evilempire.server.services
{
    class CharacterService : ICharacterService
    {
        IDbService db;
        IPlatformService platform;

        public CharacterService(IDbService db, IPlatformService platform)
        {
            this.db = db;
            this.platform = platform;
        }

        public Character GetActiveCharacter(ISession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            var character = db.SelectMany<Character, string>(session.User.Login)?.FirstOrDefault();
            if (character == null)
            {
                var characterId = db.NextValueFor(Constants.Database.Sequences.CharacterIdSequence);
                character = new Character { AssociatedLogin = session.User.Login, Id = characterId };
                db.Insert(character);
            }

            return character;
        }

        public Character GetCharacterById(int characterId)
        {
            var character = db.Select<Character, int>(ks => ks.Id, characterId);
            return character;
        }

        public CharacterCustomization GetCharacterCustomizationById(int characterId)
        {
            var characterCustomization = db.Select<CharacterCustomization, int>(characterId);
            return characterCustomization;
        }

        public CharacterCustomization CreateDefaultCharacterCustomization(int characterId)
        {
            var characterCustomization = platform.GetDefaultCharacterCustomization();
            characterCustomization.CharacterId = characterId;
            db.Insert<CharacterCustomization>(characterCustomization);
            return characterCustomization;
        }

        public void UpdatePosition(int characterId, Vector3f? position, Vector3f? rotation)
        {
            var character = GetCharacterById(characterId);
            if (position.HasValue)
            {
                character.Position = position;
            }
            if (rotation.HasValue)
            {
                character.Rotation = rotation;
            }
            db.Update<Character>(character);
        }
    }
}

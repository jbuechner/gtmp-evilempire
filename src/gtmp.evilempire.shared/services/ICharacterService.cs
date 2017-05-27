using gtmp.evilempire.entities;
using gtmp.evilempire.sessions;

namespace gtmp.evilempire.services
{
    public interface ICharacterService
    {
        Character GetActiveCharacter(ISession session);
        Character GetCharacterById(int characterId);
        CharacterCustomization GetCharacterCustomizationById(int characterId);
        CharacterCustomization CreateDefaultCharacterCustomization(int characterId);
        void UpdatePosition(int characterId, Vector3f? position, Vector3f? rotation);
    }
}

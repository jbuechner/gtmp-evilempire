using gtmp.evilempire.entities;

namespace gtmp.evilempire.services
{
    public interface ICharacterService
    {
        Character GetActiveCharacter(IClient client);
        Character GetCharacterById(int characterId);
        CharacterCustomization GetCharacterCustomizationById(int characterId);
        void UpdatePosition(int characterId, Vector3f? position, Vector3f? rotation);
    }
}

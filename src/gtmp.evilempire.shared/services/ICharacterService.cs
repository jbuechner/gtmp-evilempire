using gtmp.evilempire.entities;

namespace gtmp.evilempire.services
{
    public interface ICharacterService
    {
        Character GetActiveCharacter(IClient client);
        void UpdatePosition(int characterId, Vector3f position);
    }
}

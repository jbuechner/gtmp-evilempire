﻿using gtmp.evilempire.entities;
using gtmp.evilempire.sessions;
using System.Collections.Generic;

namespace gtmp.evilempire.services
{
    public interface ICharacterService
    {
        Character GetActiveCharacter(ISession session);
        Character GetCharacterById(int characterId);

        CharacterCustomization GetCharacterCustomizationById(int characterId);
        CharacterInventory GetCharacterInventoryById(int characterId);

        Item GetCharacterItemById(int characterId, long itemId);

        CharacterCustomization CreateDefaultCharacterCustomization(int characterId);
        CharacterInventory CreateDefaultCharacterInventory(int characterId);

        IEnumerable<Item> GetKeys(int characterId, Vehicle vehicle);

        CharacterInventoryChanges AddToCharacterInventory(int characterId, IEnumerable<Item> items);
        CharacterInventoryChanges RemoveFromCharacterInventory(int characterId, IEnumerable<Item> items);

        void UpdatePosition(int characterId, Vector3f? position, Vector3f? rotation);

        double GetTotalAmountOfMoney(int characterId, Currency currency);
    }
}

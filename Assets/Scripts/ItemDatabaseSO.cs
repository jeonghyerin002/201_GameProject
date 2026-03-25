using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabaseSO", menuName = "Inventory/DatabaseSO")]
public class ItemDatabaseSO : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();

    private Dictionary<int, ItemSO> itemsById;
    private Dictionary<string, ItemSO> itemsByName;

    public void Initalze()
    {
        itemsById = new Dictionary<int, ItemSO>();
        itemsByName = new Dictionary<string, ItemSO>();

        foreach (var item in items)
        {
            itemsById[item.id] = item;
            itemsByName[item.itemName] = item;
        }
    }
    public ItemSO GetItemById(int id)
    {
        if (itemsById == null)
        {
            Initalze();
        }
        
        if (itemsById.TryGetValue(id, out ItemSO item))
            return item;

        return null;
    }
    public ItemSO GetItemByName(string name)
    {
        if (itemsByName == null)
        {
            Initalze();
        }

        if (itemsByName.TryGetValue(name, out ItemSO item)) 
            return item;
        return null;
    }
    public List<ItemSO> GetItemByType(ItemType type)
    {
        return items.FindAll(item => item.itemType == type);
    }
}

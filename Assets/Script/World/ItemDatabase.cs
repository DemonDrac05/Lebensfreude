using System.Collections.Generic;
using UnityEngine;

// Looks up a BaseItem by name (Save/Load) or by sprite (pickup). Fill allItems in the Inspector (every item in the game).
[CreateAssetMenu(menuName = "ScriptableObjects/World/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<BaseItem> allItems = new();
    private Dictionary<string, BaseItem> _map;

    public BaseItem Find(string itemName)
    {
        if (_map == null)
        {
            _map = new Dictionary<string, BaseItem>();
            foreach (var i in allItems) if (i != null && !_map.ContainsKey(i.name)) _map[i.name] = i;
        }
        return _map.TryGetValue(itemName, out var it) ? it : null;
    }

    // Look up an item by its sprite. Used by PlayerCollision so ANY dropped item can be collected.
    public BaseItem FindBySprite(Sprite sprite)
    {
        if (sprite == null) return null;
        foreach (var i in allItems) if (i != null && i.image == sprite) return i;
        return null;
    }
}

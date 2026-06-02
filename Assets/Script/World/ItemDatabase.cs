using System.Collections.Generic;
using UnityEngine;

// Tra cứu BaseItem theo TÊN SO (cho Save/Load). Điền allItems trong Inspector (mọi item của game).
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
}

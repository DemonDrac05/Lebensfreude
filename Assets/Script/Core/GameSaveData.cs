using System;
using System.Collections.Generic;

[Serializable] public class ItemStackSave { public string item; public int count; }
[Serializable] public class VillageSave   { public int villageId; public int phase; public bool revivalUnlocked; }

// Dữ liệu lưu (JsonUtility). Mở rộng thêm field sau nếu cần (lò/rương/market state...).
[Serializable]
public class GameSaveData
{
    public int   token;
    public int   totalDays;
    public float stamina;
    public List<ItemStackSave> inventory          = new();
    public List<VillageSave>   villages           = new();
    public List<int>           artifactsEarned     = new();
    public List<int>           artifactsInserted   = new();
}

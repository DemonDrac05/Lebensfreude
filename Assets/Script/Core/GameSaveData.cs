using System;
using System.Collections.Generic;

[Serializable] public class ItemStackSave { public string item; public int count; }
[Serializable] public class VillageSave   { public int villageId; public int phase; public bool revivalUnlocked; }

[Serializable]
public class GameSaveData
{
    public int   token;
    public int   totalDays;
    public float stamina;
    
    // Lưu tọa độ người chơi thực tế
    public float playerX;
    public float playerY;
    public float playerZ;

    // Lưu hầm ngục hiện tại
    public int currentDepth;

    public List<ItemStackSave> inventory          = new();
    public List<VillageSave>   villages           = new();
    public List<int>           artifactsEarned     = new();
    public List<int>           artifactsInserted   = new();
}
using System;
using UnityEngine;

[Serializable]
public class DemandEvent
{
    // DATA
    public VillageId villageId;
    public BaseItem  item;
    public int       requiredAmount;
    public float     bonusMultiplier;
    public int       deadlineDay;

    // STATE
    public int filledAmount;

    public bool IsExpired(int currentDay) => currentDay > deadlineDay;
    public bool IsFulfilled => filledAmount >= requiredAmount;
    public int  Remaining   => Mathf.Max(0, requiredAmount - filledAmount);
}

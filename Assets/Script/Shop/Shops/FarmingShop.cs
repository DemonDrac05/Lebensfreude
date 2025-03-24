using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmingShop : Shop
{
    public override bool CanSellItem(BaseItem item)
    {
        return item is Farming;
    }
}

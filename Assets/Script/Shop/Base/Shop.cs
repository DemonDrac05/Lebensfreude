using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public virtual bool CanSellItem(BaseItem item)
    {
        return false;
    }
}

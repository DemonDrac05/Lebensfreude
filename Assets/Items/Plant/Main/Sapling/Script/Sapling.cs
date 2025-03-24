using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Plant/Saplings")]
public class Sapling : Plant
{
    public override int MaxStackable => 1;
}

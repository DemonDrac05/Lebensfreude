using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Item/Consumable")]
public class Consumable : BaseItem
{
    [Header("=== Stamina restore ==========")]
    public float staminaRestore = 15f;
}

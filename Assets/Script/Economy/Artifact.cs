using UnityEngine;

// ─────────────────────────────────────────
// ARTIFACT  (vật phẩm độc nhất — chỉ để input vào Legendary Hall)
// ─────────────────────────────────────────
// Kế thừa BaseItem nên nằm trong inventory như món thường, NHƯNG công dụng duy nhất là cắm vào Hall.
// Không stack (MaxStackable = 1). Để KHÔNG mua/bán: đặt buyingPrice = sellingPrice = -1 trên asset,
// và KHÔNG thêm Artifact vào bất kỳ list làng nào.
//
// Liên kết: VillageProgressionManager (AddItem khi hồi sinh làng), LegendaryHall (cắm/consume),
//           InventoryItem.InitialiseItem (hiện sprite), ArtifactManager (theo dõi type).
[CreateAssetMenu(menuName = "ScriptableObjects/Item/Artifact")]
public class Artifact : BaseItem
{
    [Header("=== Loại Artifact ==========")]
    public ArtifactType type = ArtifactType.Forest;

    public override int MaxStackable => 1;
}

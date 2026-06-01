using UnityEngine;

// ─────────────────────────────────────────
// LEGENDARY HALL  (cắm artifact -> mở ending)
// ─────────────────────────────────────────
// Tới gần (vào trigger) + CHỌN artifact ở toolbar rồi CLICK vào Hall để cắm (đã chốt).
// Mỗi lần cắm: trừ món đang chọn (mất) + ArtifactManager.Insert (Hall ghi nhận seal) + sáng seal + hiện lore.
// Đủ 3 seal -> EndingManager.TriggerEnding().
//
// Cần: Collider2D trên Hall (để OnMouseDown nhận click) + 1 Collider2D trigger cho vùng tới gần.
// Liên kết: InventoryManager (GetSelectedItem<Artifact>), ArtifactManager (Insert/IsInserted/AllInserted),
//           MessageOverlay (lore), EndingManager (ending).
public class LegendaryHall : MonoBehaviour
{
    // ─────────────────────────────────────────
    // CONFIG  (Inspector)
    // ─────────────────────────────────────────
    [Header("=== Lore khi cắm từng artifact ==========")]
    [TextArea] public string forestLore   = "A green seal awakens. The forest folk remember your kindness.";
    [TextArea] public string mountainLore = "An orange seal blazes. The mountain forges roar back to life.";
    [TextArea] public string goldenLore   = "A golden seal shines. The artisans' city breathes once more.";

    [Header("=== Thông báo phụ ==========")]
    [TextArea] public string noArtifactMessage      = "This door holds three seals. The villages remember.";
    [TextArea] public string alreadyInsertedMessage = "This seal already glows. Another waits.";

    [Header("=== Hiệu ứng seal (bật khi cắm) ==========")]
    [SerializeField] private GameObject sealForest;
    [SerializeField] private GameObject sealMountain;
    [SerializeField] private GameObject sealGolden;

    private bool _playerInRange;

    // ─────────────────────────────────────────
    // PROXIMITY
    // ─────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() != null) _playerInRange = true;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() != null) _playerInRange = false;
    }

    // ─────────────────────────────────────────
    // INSERT  (click vào Hall khi đang chọn 1 Artifact ở toolbar)
    // ─────────────────────────────────────────
    private void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;          // đang có overlay -> bỏ qua
        if (!_playerInRange) return;                 // phải đứng gần Hall
        if (InventoryManager.Instance == null || ArtifactManager.Instance == null) return;

        var art = InventoryManager.Instance.GetSelectedItem<Artifact>(false);
        if (art == null)
        {
            MessageOverlay.Instance?.Show(noArtifactMessage);   // không chọn artifact
            return;
        }
        if (ArtifactManager.Instance.IsInserted(art.type))
        {
            MessageOverlay.Instance?.Show(alreadyInsertedMessage);
            return;
        }

        // Cắm: trừ món đang chọn (mất) + ghi seal + sáng seal + lore.
        InventoryManager.Instance.GetSelectedItem<Artifact>(true);
        ArtifactManager.Instance.Insert(art.type);
        LightSeal(art.type);
        MessageOverlay.Instance?.Show(LoreFor(art.type), AfterLore);
    }

    // Sau khi đọc lore: đủ 3 seal thì mở ending. Dùng trong: callback của MessageOverlay.Show.
    private void AfterLore()
    {
        if (ArtifactManager.Instance != null && ArtifactManager.Instance.AllInserted)
            EndingManager.Instance?.TriggerEnding();
    }

    // ─────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────
    private void LightSeal(ArtifactType type)
    {
        GameObject seal = type switch
        {
            ArtifactType.Forest   => sealForest,
            ArtifactType.Mountain => sealMountain,
            ArtifactType.Golden   => sealGolden,
            _                     => null
        };
        if (seal != null) seal.SetActive(true);
    }

    private string LoreFor(ArtifactType type) => type switch
    {
        ArtifactType.Forest   => forestLore,
        ArtifactType.Mountain => mountainLore,
        ArtifactType.Golden   => goldenLore,
        _                     => ""
    };
}

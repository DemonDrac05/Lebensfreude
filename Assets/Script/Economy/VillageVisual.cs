using UnityEngine;

// ─────────────────────────────────────────
// VILLAGE VISUAL  (đổi ảnh làng theo phase: basic → mid → rich)
// ─────────────────────────────────────────
// Gắn lên ROOT của làng (cùng nơi có VillageMarket + Collider2D CỐ ĐỊNH).
// Sprite đặt ở 1 CHILD riêng (visualRenderer) -> đổi ảnh KHÔNG đụng tới collider -> không kẹt player.
// Nghe VillageProgressionManager.OnPhaseAdvanced để swap sprite.
//
// Map (đã chốt): Abandoned + Trust = basic; Partnership = mid; Revival = rich.
// Lưu ý setup: để pivot mọi sprite = Bottom-Center -> làng "cao lên" nhưng chân đế giữ nguyên.
public class VillageVisual : MonoBehaviour
{
    // ─────────────────────────────────────────
    // CONFIG  (Inspector)
    // ─────────────────────────────────────────
    [Header("=== Định danh ==========")]
    public VillageId villageId;

    [Header("=== Renderer ở CHILD 'Visual' (KHÔNG gắn collider vào đây) ==========")]
    public SpriteRenderer visualRenderer;

    [Header("=== 3 ảnh theo giai đoạn ==========")]
    public Sprite basicSprite;   // Abandoned + Trust
    public Sprite midSprite;     // Partnership
    public Sprite richSprite;    // Revival

    // ─────────────────────────────────────────
    // LIFECYCLE
    // ─────────────────────────────────────────
    private void OnEnable()
    {
        if (VillageProgressionManager.Instance != null)
            VillageProgressionManager.Instance.OnPhaseAdvanced += HandlePhase;
        RefreshNow();
    }

    private void OnDisable()
    {
        if (VillageProgressionManager.Instance != null)
            VillageProgressionManager.Instance.OnPhaseAdvanced -= HandlePhase;
    }

    // Áp ngay ảnh đúng phase hiện tại (phòng khi bật muộn). Dùng trong: OnEnable.
    private void RefreshNow()
    {
        var phase = VillageProgressionManager.Instance != null
            ? VillageProgressionManager.Instance.GetPhase(villageId)
            : VillagePhase.Abandoned;
        Apply(phase);
    }

    // Nghe sự kiện nâng phase; chỉ phản ứng với đúng làng của mình. Dùng trong: event OnPhaseAdvanced.
    private void HandlePhase(VillageId id, VillagePhase phase)
    {
        if (id == villageId) Apply(phase);
    }

    // Đổi sprite theo phase. KHÔNG đụng collider/transform gốc.
    private void Apply(VillagePhase phase)
    {
        if (visualRenderer == null) return;
        Sprite target = phase switch
        {
            VillagePhase.Partnership => midSprite,
            VillagePhase.Revival     => richSprite,
            _                        => basicSprite   // Abandoned + Trust
        };
        if (target != null) visualRenderer.sprite = target;
    }
}

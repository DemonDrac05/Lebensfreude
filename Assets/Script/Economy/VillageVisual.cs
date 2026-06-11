using UnityEngine;

//
public class VillageVisual : MonoBehaviour
{
    // CONFIG  (Inspector)
    [Header("=== Identity ==========")]
    public VillageId villageId;

    [Header("=== Renderer on CHILD 'Visual' (do NOT put a collider here) ==========")]
    public SpriteRenderer visualRenderer;

    [Header("=== 3 phase images ==========")]
    public Sprite basicSprite;   // Abandoned + Trust
    public Sprite midSprite;     // Partnership
    public Sprite richSprite;    // Revival

    // LIFECYCLE
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

    private void RefreshNow()
    {
        var phase = VillageProgressionManager.Instance != null
            ? VillageProgressionManager.Instance.GetPhase(villageId)
            : VillagePhase.Abandoned;
        Apply(phase);
    }

    private void HandlePhase(VillageId id, VillagePhase phase)
    {
        if (id == villageId) Apply(phase);
    }

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

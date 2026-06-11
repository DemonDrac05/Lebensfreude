using UnityEngine;

//
//           MessageOverlay (lore), EndingManager (ending).
public class LegendaryHall : MonoBehaviour
{
    // CONFIG  (Inspector)
    [Header("=== Lore when each artifact is inserted ==========")]
    [TextArea] public string forestLore   = "A green seal awakens. The forest folk remember your kindness.";
    [TextArea] public string mountainLore = "An orange seal blazes. The mountain forges roar back to life.";
    [TextArea] public string goldenLore   = "A golden seal shines. The artisans' city breathes once more.";

    [Header("=== Secondary message ==========")]
    [TextArea] public string noArtifactMessage      = "This door holds three seals. The villages remember.";
    [TextArea] public string alreadyInsertedMessage = "This seal already glows. Another waits.";

    [Header("=== Seal effect (on when inserted) ==========")]
    [SerializeField] private GameObject sealForest;
    [SerializeField] private GameObject sealMountain;
    [SerializeField] private GameObject sealGolden;

    private bool _playerInRange;

    // PROXIMITY
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() != null) _playerInRange = true;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() != null) _playerInRange = false;
    }

    private void OnMouseDown()
    {
        if (InputBlocker.IsBlocked) return;
        if (!_playerInRange) return;
        if (InventoryManager.Instance == null || ArtifactManager.Instance == null) return;

        var art = InventoryManager.Instance.GetSelectedItem<Artifact>(false);
        if (art == null)
        {
            MessageOverlay.Instance?.Show(noArtifactMessage);
            return;
        }
        if (ArtifactManager.Instance.IsInserted(art.type))
        {
            MessageOverlay.Instance?.Show(alreadyInsertedMessage);
            return;
        }

        InventoryManager.Instance.GetSelectedItem<Artifact>(true);
        ArtifactManager.Instance.Insert(art.type);
        LightSeal(art.type);
        MessageOverlay.Instance?.Show(LoreFor(art.type), AfterLore);
    }

    private void AfterLore()
    {
        if (ArtifactManager.Instance != null && ArtifactManager.Instance.AllInserted)
            EndingManager.Instance?.TriggerEnding();
    }

    // HELPERS
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

using System;
using System.Collections.Generic;
using UnityEngine;

// HINT TARGET (one village to point toward in a dream)
[Serializable]
public class HintTarget
{
    public Transform target;
    public VillageId villageId;
    [TextArea] public string hintTemplate = "Smoke rises from the {dir}... someone is still alive out there.";
}

// DREAM HINT SYSTEM (hint CONTENT only; presentation is handled by SleepManager).
// Returns text only; SleepManager shows the overlay, waits for close and blocks input.
// Content tiers:
//   - Intro lore        -> shown FIRST on a new game (via SleepManager.ShowIntro).
//   - All 3 Artifacts    -> hint the EXACT Hall location (direction + distance) on the next sleep.
//   - Otherwise          -> hint the DIRECTION to the nearest undiscovered village, by hintChance.
public class DreamHintSystem : MonoBehaviour
{
    public static DreamHintSystem Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    [Header("=== Intro lore (shown at game start) ==========")]
    [TextArea(3, 6)] public string introLore =
        "The Legendary Merchant Hall has stood sealed for a hundred years. It will open once more — for the one who restores what was lost. Three villages. Three keepers. Three locks. The world is waiting.";

    [Header("=== Hall location hint (when all 3 Artifacts collected) ==========")]
    [SerializeField] private Transform hallTarget;
    [TextArea(2, 4)] public string hallLocationTemplate =
        "The Hall stirs. It waits to the {dir}, about {dist} steps away. The door remembers the warmth of commerce. It is time.";

    [Header("=== Village direction hint ==========")]
    [Range(0f, 1f)] public float hintChance = 0.3f;
    [SerializeField] private Transform player;            // leave empty -> auto-find Player
    [SerializeField] private List<HintTarget> villageTargets = new();

    // PUBLIC API (called by SleepManager)
    public string GetIntroText() => introLore;

    // Returns the hint text for one sleep ("" = no hint, just the black screen).
    public string RollSleepHint()
    {
        // 1) All 3 Artifacts -> always hint the Hall location (highest priority).
        if (ArtifactManager.Instance != null && ArtifactManager.Instance.HasAllArtifacts && hallTarget != null)
            return BuildHallHint();

        // 2) Otherwise hint a village direction by chance.
        if (UnityEngine.Random.value > hintChance) return "";

        HintTarget t = NearestUndiscovered();
        if (t == null || t.target == null) return "";

        Transform p = ResolvePlayer();
        string dir = p != null ? CompassDirection(p.position, t.target.position) : "horizon";
        return (t.hintTemplate ?? "").Replace("{dir}", dir);
    }

    private string BuildHallHint()
    {
        Transform p = ResolvePlayer();
        string dir = p != null ? CompassDirection(p.position, hallTarget.position) : "horizon";
        int dist   = p != null ? Mathf.RoundToInt(Vector2.Distance(p.position, hallTarget.position)) : 0;
        return (hallLocationTemplate ?? "").Replace("{dir}", dir).Replace("{dist}", dist.ToString());
    }

    private Transform ResolvePlayer()
    {
        if (player == null)
        {
            var found = FindObjectOfType<Player>();
            if (found != null) player = found.transform;
        }
        return player;
    }

    private HintTarget NearestUndiscovered()
    {
        Transform p = ResolvePlayer();
        HintTarget best = null;
        float bestDist = float.MaxValue;
        foreach (var t in villageTargets)
        {
            if (t == null || t.target == null) continue;
            if (IsDiscovered(t.villageId)) continue;
            float d = p != null ? (t.target.position - p.position).sqrMagnitude : 0f;
            if (d < bestDist) { bestDist = d; best = t; }
        }
        return best;
    }

    private bool IsDiscovered(VillageId id)
        => VillageProgressionManager.Instance != null
        && VillageProgressionManager.Instance.GetPhase(id) != VillagePhase.Abandoned;

    // 8-point compass (y up = North).
    private static string CompassDirection(Vector2 from, Vector2 to)
    {
        Vector2 d = to - from;
        if (d.sqrMagnitude < 0.0001f) return "here";
        float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg; // 0 = East, 90 = North
        if (ang < 0f) ang += 360f;
        string[] dirs = { "East", "North-East", "North", "North-West", "West", "South-West", "South", "South-East" };
        return dirs[Mathf.RoundToInt(ang / 45f) % 8];
    }
}

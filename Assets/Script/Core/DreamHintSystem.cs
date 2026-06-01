using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────
// HINT TARGET  (1 làng để chỉ hướng trong giấc mơ)
// ─────────────────────────────────────────
[Serializable]
public class HintTarget
{
    public Transform target;
    public VillageId villageId;
    [TextArea] public string hintTemplate = "Smoke rises from the {dir}... someone is still alive out there.";
}

// ─────────────────────────────────────────
// DREAM HINT SYSTEM  (NỘI DUNG gợi ý — không lo trình bày)
// ─────────────────────────────────────────
// Chỉ TRẢ VỀ text; phần hiện overlay/đợi-đóng/chặn-input do SleepManager lo (tách bạch để sync).
// Tầng nội dung:
//   • Intro lore  -> thứ ĐẦU TIÊN khi vào game mới (qua SleepManager.ShowIntro).
//   • Đủ 3 Artifact -> hint VỊ TRÍ CHÍNH XÁC của Hall (hướng + khoảng cách), hiện ở LẦN NGỦ kế.
//   • Còn lại     -> hint HƯỚNG tới làng gần nhất CHƯA khám phá, theo xác suất hintChance.
//
// Liên kết: SleepManager (GetIntroText, RollSleepHint), ArtifactManager (HasAllArtifacts),
//           VillageProgressionManager (làng đã khám phá chưa).
public class DreamHintSystem : MonoBehaviour
{
    public static DreamHintSystem Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ─────────────────────────────────────────
    // CONFIG  (Inspector)
    // ─────────────────────────────────────────
    [Header("=== Intro lore (hiện đầu game) ==========")]
    [TextArea(3, 6)] public string introLore =
        "The Legendary Merchant Hall has stood sealed for a hundred years. It will open once more — for the one who restores what was lost. Three villages. Three keepers. Three locks. The world is waiting.";

    [Header("=== Hint vị trí Hall (khi đủ 3 Artifact) ==========")]
    [SerializeField] private Transform hallTarget;
    [TextArea(2, 4)] public string hallLocationTemplate =
        "The Hall stirs. It waits to the {dir}, about {dist} steps away. The door remembers the warmth of commerce. It is time.";

    [Header("=== Hint hướng làng ==========")]
    [Range(0f, 1f)] public float hintChance = 0.3f;
    [SerializeField] private Transform player;            // để trống -> tự tìm Player
    [SerializeField] private List<HintTarget> villageTargets = new();

    // ─────────────────────────────────────────
    // PUBLIC API  (gọi bởi SleepManager)
    // ─────────────────────────────────────────
    // Lore mở màn. Dùng trong: SleepManager.ShowIntro().
    public string GetIntroText() => introLore;

    // Trả text hint cho 1 lần ngủ ("" = không có hint, chỉ màn đen). Dùng trong: SleepManager.Sleep().
    public string RollSleepHint()
    {
        // 1) Đủ 3 Artifact -> luôn hint vị trí Hall (ưu tiên cao nhất).
        if (ArtifactManager.Instance != null && ArtifactManager.Instance.HasAllArtifacts && hallTarget != null)
            return BuildHallHint();

        // 2) Hint hướng làng theo xác suất.
        if (UnityEngine.Random.value > hintChance) return "";

        HintTarget t = NearestUndiscovered();
        if (t == null || t.target == null) return "";

        Transform p = ResolvePlayer();
        string dir = p != null ? CompassDirection(p.position, t.target.position) : "horizon";
        return (t.hintTemplate ?? "").Replace("{dir}", dir);
    }

    // ─────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────
    // Dựng câu hint vị trí Hall: hướng + khoảng cách (ô). Dùng trong: RollSleepHint.
    private string BuildHallHint()
    {
        Transform p = ResolvePlayer();
        string dir = p != null ? CompassDirection(p.position, hallTarget.position) : "horizon";
        int dist   = p != null ? Mathf.RoundToInt(Vector2.Distance(p.position, hallTarget.position)) : 0;
        return (hallLocationTemplate ?? "").Replace("{dir}", dir).Replace("{dist}", dist.ToString());
    }

    // Tự tìm Player nếu chưa gán. Dùng trong: RollSleepHint, BuildHallHint, NearestUndiscovered.
    private Transform ResolvePlayer()
    {
        if (player == null)
        {
            var found = FindObjectOfType<Player>();
            if (found != null) player = found.transform;
        }
        return player;
    }

    // Làng gần nhất CHƯA khám phá (phase == Abandoned). Dùng trong: RollSleepHint.
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

    // Hướng la bàn 8 phương (y lên = North). Dùng trong: RollSleepHint, BuildHallHint.
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

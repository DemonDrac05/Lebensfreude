using System.Collections.Generic;
using TMPro;
using UnityEngine;

// ─────────────────────────────────────────
// PHASE DEBUG PANEL  (UI chọn phase cho từng làng — chỉ dùng khi test)
// ─────────────────────────────────────────
// Gắn lên 1 panel trong Canvas (để bật/tắt bằng phím hoặc toggle trong Inspector).
// Mỗi làng 1 TMP_Dropdown với các option: Abandoned / Trust / Partnership / Revival.
// Thay đổi dropdown → gọi VillageProgressionManager.ForceSetPhase() ngay lập tức.
// RefreshAll() để đồng bộ dropdown về phase thực tế (sau khi game tự nâng phase).
//
// Setup trong scene: 3 TMP_Dropdown child objects, kéo vào 3 field tương ứng.
// Liên kết: VillageProgressionManager.ForceSetPhase / GetPhase.
public class PhaseDebugPanel : MonoBehaviour
{
    [Header("=== Dropdown: thứ tự Sylvan / Ironhold / Aurum ==========")]
    [SerializeField] private TMP_Dropdown sylvanDropdown;
    [SerializeField] private TMP_Dropdown ironholdDropdown;
    [SerializeField] private TMP_Dropdown aurumDropdown;

    // Tên hiển thị trong dropdown (phải khớp thứ tự enum VillagePhase: 0,1,2,3)
    private static readonly List<string> PhaseOptions = new()
        { "Abandoned (0)", "Trust (1)", "Partnership (2)", "Revival (3)" };

    // ─────────────────────────────────────────
    // INIT
    // ─────────────────────────────────────────
    private void Start()
    {
        SetupDropdown(sylvanDropdown,   VillageId.Sylvan);
        SetupDropdown(ironholdDropdown, VillageId.Ironhold);
        SetupDropdown(aurumDropdown,    VillageId.Aurum);

        // Nghe event phase thay đổi từ game (mua bán đủ ngưỡng) → tự đồng bộ dropdown
        if (VillageProgressionManager.Instance != null)
            VillageProgressionManager.Instance.OnPhaseAdvanced += OnPhaseAdvanced;
    }

    private void OnDestroy()
    {
        if (VillageProgressionManager.Instance != null)
            VillageProgressionManager.Instance.OnPhaseAdvanced -= OnPhaseAdvanced;
    }

    // ─────────────────────────────────────────
    // SETUP 1 DROPDOWN
    // ─────────────────────────────────────────
    // Điền options, set giá trị hiện tại, đăng ký callback. Dùng trong: Start().
    private void SetupDropdown(TMP_Dropdown dd, VillageId id)
    {
        if (dd == null) return;

        dd.ClearOptions();
        dd.AddOptions(PhaseOptions);

        // Hiện phase hiện tại (default Abandoned nếu chưa khởi tạo)
        int current = VillageProgressionManager.Instance != null
            ? (int)VillageProgressionManager.Instance.GetPhase(id)
            : 0;
        dd.SetValueWithoutNotify(current);

        // Listener: player chọn dropdown → ép phase
        dd.onValueChanged.AddListener(val =>
        {
            VillageProgressionManager.Instance?.ForceSetPhase(id, (VillagePhase)val);
        });
    }

    // ─────────────────────────────────────────
    // SYNC DROPDOWN ← GAME (khi game tự nâng phase)
    // ─────────────────────────────────────────
    // Nghe VillageProgressionManager.OnPhaseAdvanced để dropdown tự cập nhật. Dùng trong: Start/OnDestroy.
    private void OnPhaseAdvanced(VillageId id, VillagePhase phase)
    {
        switch (id)
        {
            case VillageId.Sylvan:    RefreshDropdown(sylvanDropdown,   id); break;
            case VillageId.Ironhold:  RefreshDropdown(ironholdDropdown, id); break;
            case VillageId.Aurum:     RefreshDropdown(aurumDropdown,    id); break;
        }
    }

    // Cập nhật 1 dropdown về phase hiện tại mà không kích callback. Dùng trong: OnPhaseAdvanced.
    private void RefreshDropdown(TMP_Dropdown dd, VillageId id)
    {
        if (dd == null || VillageProgressionManager.Instance == null) return;
        dd.SetValueWithoutNotify((int)VillageProgressionManager.Instance.GetPhase(id));
    }

    // Cập nhật tất cả dropdowns (gọi từ ngoài nếu cần). Dùng trong: (tùy ý mày).
    public void RefreshAll()
    {
        RefreshDropdown(sylvanDropdown,   VillageId.Sylvan);
        RefreshDropdown(ironholdDropdown, VillageId.Ironhold);
        RefreshDropdown(aurumDropdown,    VillageId.Aurum);
    }
}

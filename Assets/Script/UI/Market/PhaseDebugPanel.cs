using System.Collections.Generic;
using TMPro;
using UnityEngine;

//
public class PhaseDebugPanel : MonoBehaviour
{
    [Header("=== Dropdown: order Sylvan / Ironhold / Aurum ==========")]
    [SerializeField] private TMP_Dropdown sylvanDropdown;
    [SerializeField] private TMP_Dropdown ironholdDropdown;
    [SerializeField] private TMP_Dropdown aurumDropdown;

    private static readonly List<string> PhaseOptions = new()
        { "Abandoned (0)", "Trust (1)", "Partnership (2)", "Revival (3)" };

    // INIT
    private void Start()
    {
        SetupDropdown(sylvanDropdown,   VillageId.Sylvan);
        SetupDropdown(ironholdDropdown, VillageId.Ironhold);
        SetupDropdown(aurumDropdown,    VillageId.Aurum);

        if (VillageProgressionManager.Instance != null)
            VillageProgressionManager.Instance.OnPhaseAdvanced += OnPhaseAdvanced;
    }

    private void OnDestroy()
    {
        if (VillageProgressionManager.Instance != null)
            VillageProgressionManager.Instance.OnPhaseAdvanced -= OnPhaseAdvanced;
    }

    // SETUP 1 DROPDOWN
    private void SetupDropdown(TMP_Dropdown dd, VillageId id)
    {
        if (dd == null) return;

        dd.ClearOptions();
        dd.AddOptions(PhaseOptions);

        int current = VillageProgressionManager.Instance != null
            ? (int)VillageProgressionManager.Instance.GetPhase(id)
            : 0;
        dd.SetValueWithoutNotify(current);

        dd.onValueChanged.AddListener(val =>
        {
            VillageProgressionManager.Instance?.ForceSetPhase(id, (VillagePhase)val);
        });
    }

    private void OnPhaseAdvanced(VillageId id, VillagePhase phase)
    {
        switch (id)
        {
            case VillageId.Sylvan:    RefreshDropdown(sylvanDropdown,   id); break;
            case VillageId.Ironhold:  RefreshDropdown(ironholdDropdown, id); break;
            case VillageId.Aurum:     RefreshDropdown(aurumDropdown,    id); break;
        }
    }

    private void RefreshDropdown(TMP_Dropdown dd, VillageId id)
    {
        if (dd == null || VillageProgressionManager.Instance == null) return;
        dd.SetValueWithoutNotify((int)VillageProgressionManager.Instance.GetPhase(id));
    }

    public void RefreshAll()
    {
        RefreshDropdown(sylvanDropdown,   VillageId.Sylvan);
        RefreshDropdown(ironholdDropdown, VillageId.Ironhold);
        RefreshDropdown(aurumDropdown,    VillageId.Aurum);
    }
}

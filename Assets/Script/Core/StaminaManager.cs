using System;
using UnityEngine;

// ─────────────────────────────────────────
// STAMINA MANAGER  (tài nguyên khan hiếm chính)
// ─────────────────────────────────────────
// Singleton SỐNG QUA SCENE (DontDestroyOnLoad) -> stamina giữ nguyên khi vào dungeon.
// Giữ currentStamina/maxStamina, cung cấp Drain/Restore + sự kiện cho HUD và gameplay.
// Ngưỡng trạng thái theo Full Design Document (mục 4).
//
// Liên kết: SleepManager (RestoreFull khi ngủ), FoodSystem (Restore khi ăn),
//           PlayerController/ToolUsedManager (Drain khi đi/đào — hook ở bước sau),
//           HUDManager (nghe OnStaminaChanged để vẽ thanh + đổi màu).
public class StaminaManager : MonoBehaviour
{
    // ─────────────────────────────────────────
    // SINGLETON
    // ─────────────────────────────────────────
    public static StaminaManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        currentStamina = maxStamina;
    }

    // ─────────────────────────────────────────
    // DATA
    // ─────────────────────────────────────────
    [SerializeField] private float maxStamina = 100f;
    public float currentStamina;

    public float Current => currentStamina;
    public float Max     => maxStamina;
    public float Percent => maxStamina > 0f ? currentStamina / maxStamina : 0f;

    // (current, max) — HUD nghe để cập nhật thanh stamina.
    public event Action<float, float> OnStaminaChanged;
    // Bắn khi stamina chạm 0 (kiệt sức).
    public event Action OnExhausted;

    // ─────────────────────────────────────────
    // STATE  (theo ngưỡng design — dùng để đổi màu HUD / giảm tốc độ)
    // ─────────────────────────────────────────
    public enum StaminaState { Normal, Tired, Exhausted, Collapsed }

    // Trạng thái hiện tại theo % stamina. Dùng trong: HUDManager (màu), PlayerController (giảm tốc).
    public StaminaState State
    {
        get
        {
            if (currentStamina <= 0f) return StaminaState.Collapsed;
            if (currentStamina < 20f) return StaminaState.Exhausted; // 19-1: giảm 40% tốc, cấm đào/craft
            if (currentStamina < 40f) return StaminaState.Tired;     // 39-20: giảm 15% tốc
            return StaminaState.Normal;                              // 100-40
        }
    }

    // Hệ số tốc độ di chuyển theo trạng thái. Dùng trong: MovementState.FrameUpdate().
    public float MoveSpeedMultiplier => State switch
    {
        StaminaState.Tired     => 0.85f, // -15%
        StaminaState.Exhausted => 0.60f, // -40%
        StaminaState.Collapsed => 0.30f, // gần đứng yên (tránh kẹt cứng hoàn toàn)
        _                      => 1f
    };

    // Có được phép dùng tool không (Exhausted/Collapsed -> cấm). Dùng trong: ToolUsedManager.Update().
    public bool CanUseTool => State == StaminaState.Normal || State == StaminaState.Tired;

    // ─────────────────────────────────────────
    // DRAIN  (tiêu hao)
    // ─────────────────────────────────────────
    // Trừ stamina (đi lại, đào, craft...). Dùng trong: PlayerController.Move, ToolUsedManager.Use (hook sau).
    public void Drain(float amount)
    {
        if (amount <= 0f) return;
        float before = currentStamina;
        currentStamina = Mathf.Max(0f, currentStamina - amount);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        if (before > 0f && currentStamina <= 0f) OnExhausted?.Invoke();
    }

    // ─────────────────────────────────────────
    // RESTORE  (hồi phục)
    // ─────────────────────────────────────────
    // Cộng stamina (ăn, uống tonic). Dùng trong: FoodSystem.Consume().
    public void Restore(float amount)
    {
        if (amount <= 0f) return;
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    // Hồi đầy (khi ngủ). Dùng trong: SleepManager.FinishSleep().
    public void RestoreFull()
    {
        currentStamina = maxStamina;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    // Đặt stamina khi load. Dùng trong: SaveManager.Load().
    public void LoadStamina(float value)
    {
        currentStamina = Mathf.Clamp(value, 0f, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
}

using System;
using UnityEngine;
using UnityEngine.UI;

//
public class StaminaManager : MonoBehaviour
{
    // SINGLETON
    public static StaminaManager Instance { get; private set; }

    private void Awake()
    {
        // Recreated with the gameplay scene. NOT DontDestroyOnLoad: a persistent instance
        // would survive back to the menu and into the next play session still holding the
        // PREVIOUS scene's (now destroyed) slider. Writing to that dead slider threw and
        // aborted the whole load. A fresh per-scene instance always has a live slider.
        Instance = this;

        currentStamina = maxStamina;
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = maxStamina;
        }
    }

    // DATA
    [SerializeField] private float maxStamina = 100f;
    public float currentStamina;
    public Slider staminaSlider;

    public float Current => currentStamina;
    public float Max     => maxStamina;
    public float Percent => maxStamina > 0f ? currentStamina / maxStamina : 0f;

    public event Action<float, float> OnStaminaChanged;
    public event Action OnExhausted;

    public enum StaminaState { Normal, Tired, Exhausted, Collapsed }

    public StaminaState State
    {
        get
        {
            if (currentStamina <= 0f) return StaminaState.Collapsed;
            if (currentStamina < 20f) return StaminaState.Exhausted;
            if (currentStamina < 40f) return StaminaState.Tired;
            return StaminaState.Normal;                              // 100-40
        }
    }

    public float MoveSpeedMultiplier => State switch
    {
        StaminaState.Tired     => 0.85f, // -15%
        StaminaState.Exhausted => 0.60f, // -40%
        StaminaState.Collapsed => 0.30f,
        _                      => 1f
    };

    public bool CanUseTool => State == StaminaState.Normal || State == StaminaState.Tired;

    private void SetSlider(float value)
    {
        if (staminaSlider != null) staminaSlider.value = value;
    }

    public void Drain(float amount)
    {
        if (amount <= 0f) return;
        float before = currentStamina;
        currentStamina = Mathf.Max(0f, currentStamina - amount);
        SetSlider(currentStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        if (before > 0f && currentStamina <= 0f) OnExhausted?.Invoke();
    }

    public void Restore(float amount)
    {
        if (amount <= 0f) return;
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
        SetSlider(currentStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public void RestoreFull()
    {
        currentStamina = maxStamina;
        SetSlider(currentStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public void LoadStamina(float value)
    {
        currentStamina = Mathf.Clamp(value, 0f, maxStamina);
        SetSlider(currentStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
}

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class StaminaUI : MonoBehaviour
{
    private Slider staminaSlider;

    private void Awake()
    {
        staminaSlider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        if (StaminaManager.Instance != null)
        {
            StaminaManager.Instance.OnStaminaChanged += UpdateSlider;
            UpdateSlider(StaminaManager.Instance.Current, StaminaManager.Instance.Max);
        }
    }

    private void Start()
    {
        if (StaminaManager.Instance != null)
        {
            UpdateSlider(StaminaManager.Instance.Current, StaminaManager.Instance.Max);
        }
    }

    private void OnDisable()
    {
        if (StaminaManager.Instance != null)
        {
            StaminaManager.Instance.OnStaminaChanged -= UpdateSlider;
        }
    }

    private void UpdateSlider(float current, float max)
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = max;
            staminaSlider.value = current;
        }
    }
}
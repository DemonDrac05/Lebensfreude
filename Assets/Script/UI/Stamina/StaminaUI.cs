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
        // Lắng nghe sự kiện thay đổi Stamina
        if (StaminaManager.Instance != null)
        {
            StaminaManager.Instance.OnStaminaChanged += UpdateSlider;
            // Cập nhật giá trị ban đầu
            UpdateSlider(StaminaManager.Instance.Current, StaminaManager.Instance.Max);
        }
    }

    private void Start()
    {
        // Đề phòng trường hợp StaminaManager khởi động sau OnEnable
        if (StaminaManager.Instance != null)
        {
            UpdateSlider(StaminaManager.Instance.Current, StaminaManager.Instance.Max);
        }
    }

    private void OnDisable()
    {
        // Hủy lắng nghe khi bị vô hiệu hóa để tránh rò rỉ bộ nhớ (Memory Leak)
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
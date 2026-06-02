using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class InputManager : MonoBehaviour
{
    [Header("=== >UI< GameObjects Requirement ==========")]
    public GameObject toolBar;
    public GameObject background;
    private GameObject activeUI = null;

    [Header("=== >Keys< Input Mappings ==========")]
    public List<KeyMappings> keyMappings = new List<KeyMappings>();

    public static InputManager Instance;

    private void Awake() => Instance = this;

    private void OnValidate()
    {
        foreach (var mapping in keyMappings)
        {
            if (mapping.UIElement != null)
            {
                mapping.nameOfUI = $"=== {mapping.UIElement.name} ==========";
            }
        }
    }

    private void Update()
    {
        if (InputBlocker.IsBlocked) return; // đang ngủ/dream/intro -> chặn toggle panel

        foreach (var key in keyMappings)
        {
            if (key.keyMethod == KeyInputMethod.Press)
            {
                if (Input.GetKeyDown(key.keyCode))
                {
                    KeyPressMethod(key.UIElement);
                }
            }
            else if (key.keyMethod == KeyInputMethod.Hold)
            {
                KeyHoldMethod(key.keyCode, key.UIElement);
            }
        }
        ChangeBackgroundColor();
    }

    // Bật/tắt panel theo phím nhấn. Xử lý 3 trường hợp:
    //  - Chưa mở gì             -> mở panel, ẩn toolbar
    //  - Đang mở đúng panel này -> đóng panel, hiện lại toolbar
    //  - Đang mở panel KHÁC     -> đóng panel cũ rồi mở panel mới (trước đây bị kẹt, không chuyển được)
    // Dùng trong: InputManager.Update().
    private void KeyPressMethod(GameObject UIInput)
    {
        // Đóng CraftingStationUI nếu đang mở (mở bằng click world, không qua keyMappings)
        CraftingStationUI.CloseIfOpen();
        ProcessingStationUI.CloseIfOpen();
        VillageMarketUI.CloseIfOpen();
        GameObject uiToToggle = UIInput;

        // Đang mở đúng panel này -> đóng lại
        if (activeUI == uiToToggle)
        {
            activeUI.SetActive(false);
            activeUI = null;
            toolBar.SetActive(true);
            return;
        }

        // Đang mở panel khác -> đóng panel cũ trước khi mở panel mới
        if (activeUI != null)
        {
            activeUI.SetActive(false);
        }

        // Mở panel mới, ẩn toolbar
        uiToToggle.SetActive(true);
        activeUI = uiToToggle;
        toolBar.SetActive(false);
    }
    private void KeyHoldMethod(KeyCode keyCode,GameObject UIInput)
    {
        GameObject uiToToggle = UIInput;
        if (Input.GetKeyDown(keyCode))
        {
            if (activeUI == null)
            {
                uiToToggle.SetActive(true);
                activeUI = uiToToggle;
                toolBar.SetActive(false);
            }
            else
            {
                toolBar.SetActive(false);
            }
        }
        else if (Input.GetKeyUp(keyCode))
        {
            if (activeUI == uiToToggle)
            {
                activeUI.SetActive(false);
                activeUI = null;

                toolBar.SetActive(true);
            }
        }
    }

    private void ChangeBackgroundColor()
    {
        Color color = background.GetComponent<Image>().color;
        color.a = toolBar.activeSelf ? (0f / 255f) : (200f / 255f);
        background.GetComponent<Image>().color = color;
    }

    // Đóng panel đang mở (nếu có) + hiện lại toolbar. Dùng trong: SleepManager.BeginOverlay().
    public void ForceCloseActivePanel()
    {
        if (activeUI != null)
        {
            activeUI.SetActive(false);
            activeUI = null;
        }
        if (toolBar != null) toolBar.SetActive(true);
    }

    [System.Serializable]
    public class KeyMappings
    {
        public string nameOfUI;
        public KeyCode keyCode;
        public KeyInputMethod keyMethod;
        public GameObject UIElement;
    }
}

public enum KeyInputMethod { Hold, Press }
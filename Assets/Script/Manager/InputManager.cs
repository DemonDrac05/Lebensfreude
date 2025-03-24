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

    private void KeyPressMethod(GameObject UIInput)
    {
        GameObject uiToToggle = UIInput;
        if (activeUI == null)
        {
            uiToToggle.SetActive(true);
            activeUI = uiToToggle;
            toolBar.SetActive(false);
        }
        else if (activeUI == uiToToggle)
        {
            activeUI.SetActive(false);
            activeUI = null;
            toolBar.SetActive(true);
        }
        else
        {
            toolBar.SetActive(false);
        }
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
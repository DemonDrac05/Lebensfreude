using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class Chest : MonoBehaviour
{
    [Header("=== GameObject Requirement ==========")]
    public GameObject chestSlot;
    public GameObject chestUIBoard;

    private GameObject currentUIBoard;
    private GameObject copyMainInventory;

    [Header("=== Inventory Slots ==========")]
    private ChestSlot[] copySlots;
    private ChestSlot[] mainSlots;
    private ChestSlot[] secondSlots;

    [Header("=== Propertie Settings ==========")]
    private bool isOpen = false;
    private static int chestIndex = 0;
    private const float YOffset = 0.5f;

    private PlayerController tileControl;
    private Collider2D _col;

    private void Awake()
    {
        tileControl = FindObjectOfType<PlayerController>();
        _col = GetComponent<Collider2D>();
    }

    private void Start() => chestIndex = 0;

    private void Update()
    {
        if (InputBlocker.IsBlocked) return; // đang ngủ/overlay -> không tương tác rương

        if (Input.GetMouseButtonDown(1) && IsMouseOverChest())
        {
            StartCoroutine(OpenChest());
        }

        if (isOpen && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)))
        {
            StartCoroutine(CloseChest());
        }
    }

    // Chuột có nằm trên collider của rương ĐÃ ĐẶT không?
    // Dùng Collider2D.OverlapPoint thay vì so alpha + vị trí chính xác -> KHÔNG phụ thuộc
    // offset đặt đồ hay alpha animation (sửa bug: đóng rương xong không mở lại được = "bị disable").
    private bool IsMouseOverChest()
    {
        if (_col == null || !_col.enabled) return false; // ghost preview có collider tắt -> bỏ qua
        Vector3 m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        m.z = 0f;
        return _col.OverlapPoint(m);
    }

    private IEnumerator OpenChest()
    {
        if (!isOpen)
        {
            yield return PlayAnimation("Open");
            isOpen = true;
            ManageChestUI(true);
        }
    }

    private IEnumerator CloseChest()
    {
        if (isOpen)
        {
            yield return PlayAnimation("Close");
            isOpen = false;
            ManageChestUI(false);
        }
    }

    private IEnumerator PlayAnimation(string animationName)
    {
        Animator animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component missing from chest.");
            yield break;
        }

        animator.Play(animationName);
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && !animator.IsInTransition(0));
    }

    // Manage the Chest UI Board
    private void ManageChestUI(bool isOpen)
    {
        if (isOpen)
        {
            OpenChestUI();
        }
        else
        {
            CloseChestUI();
        }
    }

    private void OpenChestUI()
    {
        if (currentUIBoard == null)
        {
            CreateChestUIBoard();
        }
        else if (currentUIBoard != null)
        {
            StartCoroutine(CopyInventory());
            ToggleChestUI(true);
        }
    }

    private void CloseChestUI()
    {
        if (currentUIBoard != null)
        {
            //Mirror changes to inventory
            for (int i = 0; i < InventoryManager.Instance.ToolbarSlots.Length; i++)
            {
                var copySlot = copySlots[i];
                var toolBarSlot = InventoryManager.Instance.ToolbarSlots[i];
                MirrorSlots(copySlot, toolBarSlot);
            }
            //Mirr changes to toolbar
            for (int i = 0; i < InventoryManager.Instance.MainInventorySlots.Length; i++)
            {
                var copySlot = copySlots[i];
                var mainInventorySlot = InventoryManager.Instance.MainInventorySlots[i];
                MirrorSlots(copySlot, mainInventorySlot);
            }

            ToggleChestUI(false);
        }
    }

    private void CreateChestUIBoard()
    {
        GameObject canvas = GameObject.Find("== Chests ==========");
        GameObject chest = Instantiate(new GameObject(), canvas.transform);
        chest.name = "[Chest]_MainUI";

        currentUIBoard = Instantiate(chestUIBoard, chest.transform);
        currentUIBoard.name = "[Chest]_UIBoard";
        currentUIBoard.SetActive(true);

        PositionUI(currentUIBoard, new Vector3(0f, 250f));

        ImplementChestInventory(chest.transform);
    }

    private void ToggleChestUI(bool state)
    {
        currentUIBoard.SetActive(state);
        copyMainInventory.SetActive(state);

    }

    private void PositionUI(GameObject uiElement, Vector3 position)
    {
        RectTransform rectTransform = uiElement.GetComponent<RectTransform>();
        rectTransform.localPosition = position;
    }

    private void ImplementChestInventory(Transform chestUI)
    {
        copyMainInventory = Instantiate(InventoryManager.Instance.mainInventory, chestUI);
        copyMainInventory.name = "[Chest]_CopyMainInventory";
        copyMainInventory.SetActive(true);

        PositionUI(copyMainInventory, new Vector2(0f, -150f));

        SetupInventorySlots();
    }

    private void SetupInventorySlots()
    {
        GameObject firstRow = copyMainInventory.transform.Find("MainSlots").gameObject;
        GameObject secondRow = copyMainInventory.transform.Find("SecondSlots").gameObject;

        mainSlots = ReplaceInventorySlots(firstRow);
        secondSlots = ReplaceInventorySlots(secondRow);

        copySlots = CombineChestSlots(mainSlots, secondSlots);
        StartCoroutine(CopyInventory());
    }

    private ChestSlot[] ReplaceInventorySlots(GameObject slotParent)
    {
        InventorySlot[] oldSlots = slotParent.GetComponentsInChildren<InventorySlot>();
        foreach (var slot in oldSlots)
        {
            Destroy(slot.gameObject);
            Instantiate(chestSlot, slotParent.transform);
        }
        return slotParent.GetComponentsInChildren<ChestSlot>();
    }

    private ChestSlot[] CombineChestSlots(ChestSlot[] main, ChestSlot[] secondary)
    {
        List<ChestSlot> combinedSlots = new List<ChestSlot>(main);
        combinedSlots.AddRange(secondary);
        return combinedSlots.ToArray();
    }

    private IEnumerator CopyInventory()
    {
        yield return new WaitForEndOfFrame();

        InventoryManager.Instance.SyncFirstRowFromToolbar(); // hàng đầu khớp toolbar trước khi copy

        for (int i = 0; i < InventoryManager.Instance.MainInventorySlots.Length; i++)
        {
            var copySlot = copySlots[i];
            var mainInventorySlot = InventoryManager.Instance.MainInventorySlots[i];
            MirrorSlots(mainInventorySlot, copySlot);
        }
    }

    public void MirrorSlots(BaseSlot sourceSlot, BaseSlot targetSlot)
    {
        if (targetSlot.transform.childCount > 0)
        {
            for (int i = targetSlot.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(targetSlot.transform.GetChild(i).gameObject);
            }
        }
        if (sourceSlot.transform.childCount > 0)
        {
            // includeInactive=true: cùng lý do như InventoryManager.MirrorSlots —
            // slot nguồn có thể nằm trong panel đang inactive → phải tìm cả inactive.
            var sourceItem = sourceSlot.GetComponentInChildren<InventoryItem>(true);
            if (sourceItem == null) return;
            
            var newItem = Instantiate(sourceItem, targetSlot.transform);
            
            newItem.InitialiseItem(sourceItem.GetItem<BaseItem>());
            newItem.count = sourceItem.count;
            newItem.RefreshCount();

            newItem.name = sourceItem.gameObject.name.Replace("(Clone)", "");
            
            newItem.GetComponent<Image>().enabled = true;
            newItem.enabled = true;
        }
    }
}

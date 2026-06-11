using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MarketSellSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI nameText, priceText, ownedText;

    private BaseItem _item; private VillageMarket _market; private VillageMarketUI _ui;

    public void Setup(BaseItem item, VillageMarket market, VillageMarketUI ui)
    {
        _item = item; _market = market; _ui = ui;
        if (itemIcon != null) { itemIcon.sprite = item.image; itemIcon.enabled = item.image != null; }
        if (nameText != null) nameText.text = item.name;
        Refresh();
    }

    public void Refresh()
    {
        if (_item == null || _market == null) return;
        if (priceText != null) priceText.text = _market.GetSellPrice(_item) + " ¢";
        int owned = InventoryManager.Instance != null ? InventoryManager.Instance.CountItem(_item) : 0;
        if (ownedText != null) ownedText.text = $"(Have: {owned})";
    }

    public void OnPointerClick(PointerEventData e)
    {
        int qty = e.button == PointerEventData.InputButton.Right ? 5 : 1;
        int owned = InventoryManager.Instance != null ? InventoryManager.Instance.CountItem(_item) : 0;
        qty = Mathf.Min(qty, owned);
        if (qty <= 0) return;
        if (_market.SellFromInventory(_item, qty)) _ui.RefreshAll();
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MarketBuySlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI nameText, priceText, stockText;

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
        if (priceText != null) priceText.text = _market.GetBuyPrice(_item) + " ¢";
        if (stockText != null) stockText.text = $"(Remaining: {(_market.State != null ? _market.State.RemainingSellStock(_item) : 0)})";
    }

    public void OnPointerClick(PointerEventData e)
    {
        int qty = e.button == PointerEventData.InputButton.Right ? 5 : 1;
        bool any = false;
        for (int i = 0; i < qty; i++) { if (_market.BuyFromVillage(_item, 1)) any = true; else break; }
        if (any) _ui.RefreshAll();
    }
}

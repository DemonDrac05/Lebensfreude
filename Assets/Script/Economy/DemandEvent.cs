using System;
using UnityEngine;

// ─────────────────────────────────────────
// DEMAND EVENT  (đơn hàng "wanted" của 1 làng trong ngày)
// ─────────────────────────────────────────
// Dữ liệu thuần. Một làng muốn X món nào đó trước hạn -> trả thưởng nhân hệ số (vd 2x).
// Dùng trong: DemandEventManager (tạo/kiểm tra/hết hạn), DemandEventUI (hiển thị).
[Serializable]
public class DemandEvent
{
    // ─────────────────────────────────────────
    // DATA
    // ─────────────────────────────────────────
    public VillageId villageId;
    public BaseItem  item;
    public int       requiredAmount;
    public float     bonusMultiplier;  // 2.0 = trả gấp đôi giá gốc cho phần khớp
    public int       deadlineDay;      // hết hạn sau ngày này (TotalDays)

    // ─────────────────────────────────────────
    // STATE
    // ─────────────────────────────────────────
    public int filledAmount;

    public bool IsExpired(int currentDay) => currentDay > deadlineDay;
    public bool IsFulfilled => filledAmount >= requiredAmount;
    public int  Remaining   => Mathf.Max(0, requiredAmount - filledAmount);
}

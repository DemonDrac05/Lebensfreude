// ─────────────────────────────────────────
// INPUT BLOCKER  (cờ khoá toàn bộ input gameplay)
// ─────────────────────────────────────────
// Khi overlay ngủ/dream/intro đang hiện -> bật cờ này để chặn MỌI input gameplay,
// chỉ overlay nhận input (click để đóng). Tránh cascade phím E (vừa ngủ vừa mở inventory...).
// Dùng trong: SleepManager (bật/tắt), InputManager + Bonfire + ToolUsedManager (kiểm tra ở đầu Update).
public static class InputBlocker
{
    public static bool IsBlocked;
}

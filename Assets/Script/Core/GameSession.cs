/// <summary>
/// Scene-independent flags that carry intent across a scene change without needing a
/// MonoBehaviour to survive. They tell the gameplay (Overworld) scene HOW it was entered:
///   - New Game  -> start fresh and seed the inventory with InventoryManager.initialItems.
///   - Load Game -> restore everything from the save file and DO NOT seed initial items.
///
/// A plain static is enough: it lives for the whole play session and is reset automatically
/// when the editor leaves Play mode, so there is nothing to clean up by hand.
/// </summary>
public static class GameSession
{
    /// <summary>
    /// True only while a Load-Game is in flight. The main menu sets it just before loading
    /// the gameplay scene; SaveManager clears it once the restore has finished.
    /// </summary>
    public static bool IsLoadingSave = false;
}

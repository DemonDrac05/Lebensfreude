using UnityEngine;

// In-game quick guide / tutorial overlay. Self-creates at runtime (NO scene setup needed).
// Shown at startup and toggled with [H]. Addresses the "no tutorial / user guide" feedback and
// helps the player distinguish intended mechanics from bugs during the demo.
public class HelpOverlay : MonoBehaviour
{
    private bool _open = false; // hidden by default; press H to toggle (not an intrusive pop-up)

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        var go = new GameObject("HelpOverlay");
        go.AddComponent<HelpOverlay>();
        DontDestroyOnLoad(go);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) _open = !_open;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 24), "Press [H] for help");
        if (!_open) return;

        float w = 470f, h = 330f;
        float x = (Screen.width - w) * 0.5f;
        float y = (Screen.height - h) * 0.5f;
        GUI.Box(new Rect(x, y, w, h), "ECONOMIC STRATEGY GAME - QUICK GUIDE");

        GUILayout.BeginArea(new Rect(x + 16, y + 32, w - 32, h - 44));
        GUILayout.Label("GOAL: Revive the 3 villages, collect their 3 Artifacts and open the");
        GUILayout.Label("Legendary Merchant Hall - in as few in-game days as possible.");
        GUILayout.Space(8);
        GUILayout.Label("CONTROLS:");
        GUILayout.Label("  - Move:  WASD / Arrow keys");
        GUILayout.Label("  - Use tool / mine / chop:  Left mouse button");
        GUILayout.Label("  - Inventory (+ hand-craft):  I");
        GUILayout.Label("  - Open a village market:  Right-click the village");
        GUILayout.Label("  - Collect drops:  walk over them");
        GUILayout.Label("  - Restore stamina / advance the day:  sleep at a bonfire");
        GUILayout.Space(8);
        GUILayout.Label("TIP: Villages specialise - the same good sells for more at the village");
        GUILayout.Label("that values it. Do not flood one market; prices drop and recover slowly.");
        GUILayout.Space(8);
        GUILayout.Label("Press [H] to close.");
        GUILayout.EndArea();
    }
}

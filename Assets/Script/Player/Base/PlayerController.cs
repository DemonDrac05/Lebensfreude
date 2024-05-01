using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [field: HideInInspector] public Player player;

    [field: SerializeField] private Tilemap tilemap;
    [field: SerializeField] private GameObject hitBoxPrefab;

    [field: HideInInspector] private GameObject currentHitBox;

    void Start() => player = GetComponent<Player>();

    public Vector3 mousePosUpdate;
    public Vector3Int playerPosUpdate;

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int mouseGridPos = tilemap.WorldToCell(mousePos);
        Vector3 snappedMousePos = tilemap.GetCellCenterWorld(mouseGridPos);

        Vector3Int playerGridPos = tilemap.WorldToCell(transform.position);
        Vector3 snappedPlayerPos = tilemap.GetCellCenterWorld(playerGridPos);

        Vector3 snappedPlayerRightPos = new(snappedPlayerPos.x + 1f, snappedPlayerPos.y, 0f);
        Vector3 snappedPlayerLeftPos = new(snappedPlayerPos.x - 1f, snappedPlayerPos.y, 0f);
        Vector3 snappedPlayerUpPos = new(snappedPlayerPos.x, snappedPlayerPos.y, 0f);
        Vector3 snappedPlayerDownPos = new(snappedPlayerPos.x, snappedPlayerPos.y - 1f, 0f);

        snappedMousePos.x = Mathf.Clamp(snappedMousePos.x, snappedPlayerLeftPos.x, snappedPlayerRightPos.x);
        snappedMousePos.y = Mathf.Clamp(snappedMousePos.y, snappedPlayerDownPos.y, snappedPlayerUpPos.y);

        if (currentHitBox == null)
            currentHitBox = Instantiate(hitBoxPrefab, snappedMousePos, Quaternion.identity);
        else
            currentHitBox.transform.position = snappedMousePos;

        mousePosUpdate = snappedMousePos;
        playerPosUpdate = playerGridPos;
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Destroy(currentHitBox);
            currentHitBox = null;
        }
    }

}

using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [Header("=== Private Properties ==========")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private GameObject redHitBoxPrefab;
    [SerializeField] private GameObject greenHitBoxPrefab;
    [SerializeField] private GameObject itemPrefab;

    private CraftingItem currentCraftingItem;
    private GameObject currentHitBoxPrefab;
    private GameObject currentHitBox;
    private GameObject currentItem;

    [Header("=== Position Update ==========")]
    public Vector3 itemPosUpdate;
    public Vector3 mousePosUpdate;
    public Vector3Int playerPosUpdate;
    public Vector3 surroundingPlayerTile;

    private void Awake() 
    {
        currentHitBoxPrefab = redHitBoxPrefab;
    }

    private void Update()
    {
        CheckForItemPrefab();

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 snappedMousePos = GetSnappedMousePosition(mousePos);
        Vector3 initialMousePos = snappedMousePos;

        UpdateHitbox(snappedMousePos, initialMousePos);

        itemPosUpdate = snappedMousePos;
        itemPosUpdate.y -= 0.5f;

        if (currentItem != null)
        {
            currentItem.transform.position = itemPosUpdate;
        }

        mousePosUpdate = snappedMousePos;
        playerPosUpdate = tilemap.WorldToCell(transform.position);

        if (currentCraftingItem != null) InstantiateByTile(snappedMousePos);
        else if (currentCraftingItem == null && itemTile.Count > 0)
        {
            itemTile.Clear();
            foreach (var hitboxPos in itemTiles.Values) Destroy(hitboxPos);
            itemTiles.Clear();

            ResetHitBox();
        }
    }

    private Vector3 GetSnappedMousePosition(Vector3 mousePos)
    {
        Vector3Int mouseGridPos = tilemap.WorldToCell(mousePos);
        Vector3 snappedMousePos = tilemap.GetCellCenterWorld(mouseGridPos);

        Vector3Int playerGridPos = tilemap.WorldToCell(transform.position);
        Vector3 snappedPlayerPos = tilemap.GetCellCenterWorld(playerGridPos);

        Vector3 surroundingTile = snappedMousePos;
        surroundingTile.x = Mathf.Clamp(snappedMousePos.x, snappedPlayerPos.x - 1f, snappedPlayerPos.x + 1f);
        surroundingTile.y = Mathf.Clamp(snappedMousePos.y, snappedPlayerPos.y - 1f, snappedPlayerPos.y + 1f);

        if (itemPrefab == null)
        {
            snappedMousePos.x = surroundingTile.x;
            snappedMousePos.y = surroundingTile.y;
        }
        else if (itemPrefab != null)
        {
            if (snappedMousePos == surroundingTile)
            {
                currentHitBoxPrefab = snappedMousePos == snappedPlayerPos ? redHitBoxPrefab : greenHitBoxPrefab;
            }
            else if (snappedMousePos != surroundingTile)
            {
                currentHitBoxPrefab = redHitBoxPrefab;
            }
            Destroy(currentHitBox);
            currentHitBox = null;
            currentHitBox = Instantiate(currentHitBoxPrefab, snappedMousePos, Quaternion.identity);
        }

        surroundingPlayerTile = surroundingTile;

        return snappedMousePos;
    }

    private void UpdateHitbox(Vector3 snappedMousePos, Vector3 initialMousePos)
    {
        if (currentHitBox == null)
        {
            currentHitBoxPrefab = (itemPrefab != null && initialMousePos == tilemap.GetCellCenterWorld(tilemap.WorldToCell(transform.position)))
                                  ? redHitBoxPrefab : greenHitBoxPrefab;
            if (itemPrefab == null) currentHitBoxPrefab = redHitBoxPrefab;

            currentHitBox = Instantiate(currentHitBoxPrefab, snappedMousePos, Quaternion.identity);
        }
        else
        {
            if (itemPrefab != null && initialMousePos != currentHitBox.transform.position)
            {
                Destroy(currentHitBox);
                currentHitBoxPrefab = initialMousePos == tilemap.GetCellCenterWorld(tilemap.WorldToCell(transform.position))
                                      ? redHitBoxPrefab : greenHitBoxPrefab;

                currentHitBox = Instantiate(currentHitBoxPrefab, snappedMousePos, Quaternion.identity);
            }
            else
            {
                currentHitBox.transform.position = snappedMousePos;
            }
        }
    }

    public List<Vector3> itemsOnGround = new List<Vector3>();

    public List<List<Vector3>> itemTile = new List<List<Vector3>>();

    public Dictionary<Vector3, GameObject> itemTiles = new Dictionary<Vector3, GameObject>();

    private void InstantiateByTile(Vector3 snappedMousePos)
    {
        Vector3 cpySnappedMousePos = snappedMousePos;

        itemTile.Clear();
        for (int i = 0; i < currentCraftingItem.column; i++)
        {
            itemTile.Add(new List<Vector3>());
        }

        if (currentCraftingItem != null && currentCraftingItem.placeable)
        {
            int col = currentCraftingItem.column;
            int row = currentCraftingItem.row;

            for (int i = 0; i < col; i++)
            {
                if (i > 0)
                    cpySnappedMousePos.y -= 1f;

                for (int j = 0; j < row; j++)
                {
                    if (j > 0)
                        cpySnappedMousePos.x += 1f;

                    itemTile[i].Add(cpySnappedMousePos);

                    if (!itemTiles.ContainsKey(cpySnappedMousePos))
                    {
                        GameObject newTile = Instantiate(GetHitBoxPrefab(cpySnappedMousePos), cpySnappedMousePos, Quaternion.identity);
                        itemTiles.Add(cpySnappedMousePos, newTile);
                    }
                    else
                    {
                        GameObject existingTile = itemTiles[cpySnappedMousePos];
                        existingTile.transform.position = cpySnappedMousePos;
                    }
                }
                cpySnappedMousePos.x = snappedMousePos.x;
            }
        }

        List<Vector3> tilesToDestroy = new List<Vector3>();
        List<Vector3> tilesRemain = new List<Vector3>();

        foreach (var key in itemTiles.Keys)
        {
            if (!itemTile.Any(colList => colList.Contains(key)))
                tilesToDestroy.Add(key);
            else
                tilesRemain.Add(key);
        }

        foreach (var tilePos in tilesToDestroy)
        {
            Destroy(itemTiles[tilePos]);
            itemTiles.Remove(tilePos);
        }

        foreach (var tilePos in tilesRemain)
        {
            Destroy(itemTiles[tilePos]);
            itemTiles.Remove(tilePos);

            GameObject newTile = Instantiate(GetHitBoxPrefab(tilePos), tilePos, Quaternion.identity);
            itemTiles.Add(tilePos,newTile);
        }
    }

    private GameObject GetHitBoxPrefab(Vector3 position)
    {
        if (itemTile[0][0] == surroundingPlayerTile)
        {
            Vector3 itemOffSet = new Vector3(position.x, position.y - 0.5f,position.z);
            if (position == tilemap.GetCellCenterWorld(tilemap.WorldToCell(transform.position))
                || itemsOnGround.Contains(itemOffSet))
                return redHitBoxPrefab;
            else
                return greenHitBoxPrefab;
        }
        return redHitBoxPrefab;
    }

    private void CheckForItemPrefab()
    {
        var itemInHand = InventoryManager.Instance.GetSelectedItem<CraftingItem>(false);
        if (itemInHand != null && itemInHand.placeable)
        {
            if (itemPrefab == null || currentItem == null)
            {
                currentCraftingItem = itemInHand;
                itemPrefab = itemInHand.gameObj;

                if (currentItem != null)
                {
                    Destroy(currentItem);
                }

                CreateNewPrefab();
            }
        }
        else
        {
            ResetItemPrefabs();
        }
    }

    private void CreateNewPrefab()
    {
        currentItem = Instantiate(itemPrefab, itemPosUpdate, Quaternion.identity);
        SetPrefabProperties(currentItem, false, 0.5f);
    }

    private void ResetItemPrefabs()
    {
        if (currentItem != null)
        {
            Destroy(currentItem);
        }
        itemPrefab = null;
        currentItem = null;
        currentCraftingItem = null;
    }

    private void SetPrefabProperties(GameObject prefab, bool colliderState, float alpha)
    {
        prefab.GetComponent<BoxCollider2D>().enabled = colliderState;

        Color color = prefab.GetComponent<SpriteRenderer>().color;
        color.a = alpha;
        prefab.GetComponent<SpriteRenderer>().color = color;
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0) && currentItem != null && currentHitBoxPrefab == greenHitBoxPrefab)
        {
            GameObject newObject = Instantiate(itemPrefab, itemPosUpdate, Quaternion.identity);
            SetPrefabProperties(newObject, true, 1f);
            itemsOnGround.Add(itemPosUpdate);

            InventoryManager.Instance.GetSelectedItem<CraftingItem>(true);
            itemTile.Clear();
            foreach( var hitboxPos in itemTiles.Values) Destroy(hitboxPos);
            itemTiles.Clear();

            ResetItemPrefabs();
            ResetHitBox();
        }
    }

    private void ResetHitBox()
    {
        currentHitBoxPrefab = redHitBoxPrefab;

        if (currentHitBox != null)
        {
            Destroy(currentHitBox);
        }
        currentHitBox = null;
    }
}

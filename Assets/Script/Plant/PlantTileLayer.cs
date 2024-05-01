using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlantTileLayer : MonoBehaviour
{
    public Tilemap tilemap;

    void Update()
    {
        Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            
            Debug.Log("Player stepped into square at grid position: " + cellPosition);
        }
    }
}

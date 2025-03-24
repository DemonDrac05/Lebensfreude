using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class WateringPot
{
    private ToolUsedManager toolUsedManager;
    private PlayerController control;
    private Tilemap tilemap;

    private List<Vector3> wateredPlots;
    private Slider waterSlider;
    private float maxAmountOfWater;
    private float currentAmountOfWater;
    private LayerMask waterTerrain;
    private Color wateredGroundColor;

    public WateringPot(ToolUsedManager toolUsedManager, PlayerController control, Tilemap tilemap,
        List<Vector3> wateredPlots, float maxAmountOfWater,
        LayerMask waterTerrain, Color wateredGroundColor)
    {
        this.toolUsedManager = toolUsedManager;
        this.control = control;
        this.tilemap = tilemap;

        this.wateredPlots = wateredPlots;
        this.maxAmountOfWater = maxAmountOfWater;
        this.waterTerrain = waterTerrain;
        this.wateredGroundColor = wateredGroundColor;
    }

    public void Execute(Vector3Int mouseGridPos)
    {
        currentAmountOfWater = toolUsedManager.currentAmountOfWater;

        if (currentAmountOfWater > 0f)
        {
            currentAmountOfWater -= 20f;
            if (toolUsedManager.IsReadyToWater() && !toolUsedManager.IsWatered(control.mousePosUpdate))
            {
                tilemap.SetTileFlags(mouseGridPos, TileFlags.None);
                tilemap.SetColor(mouseGridPos, wateredGroundColor);
                wateredPlots.Add(control.mousePosUpdate);
            }
        }

        if (toolUsedManager.IsReadyToCultivate(control.mousePosUpdate, waterTerrain))
        {
            currentAmountOfWater = maxAmountOfWater;
        }

        toolUsedManager.currentAmountOfWater = currentAmountOfWater;
        waterSlider = toolUsedManager.waterSlider;
        if (waterSlider != null)
        {
            waterSlider.value = toolUsedManager.currentAmountOfWater;
        }
    }
}

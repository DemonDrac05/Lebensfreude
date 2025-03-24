using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Hoe
{
    private ToolUsedManager toolUsedManager;
    private PlayerController control;
    private Tilemap tilemap;

    private List<Vector3> cultivatedPlots;
    private LayerMask soilableGround;
    private Color soiledGroundColor;

    public Hoe(ToolUsedManager toolUsedManager, PlayerController control, Tilemap tilemap, List<Vector3> cultivatedPlots, LayerMask soilableGround, Color soiledGroundColor)
    {
        this.toolUsedManager = toolUsedManager;
        this.control = control;
        this.tilemap = tilemap;

        this.cultivatedPlots = cultivatedPlots;
        this.soilableGround = soilableGround;
        this.soiledGroundColor = soiledGroundColor;
    }

    public void Execute(Vector3Int mouseGridPos)
    {
        if (toolUsedManager.IsReadyToCultivate(control.mousePosUpdate, soilableGround) && !toolUsedManager.IsWatered(control.mousePosUpdate))
        {
            tilemap.SetTileFlags(mouseGridPos, TileFlags.None);
            tilemap.SetColor(mouseGridPos, soiledGroundColor);

            if (!cultivatedPlots.Contains(control.mousePosUpdate))
            {
                cultivatedPlots.Add(control.mousePosUpdate);
            }
        }
    }
}

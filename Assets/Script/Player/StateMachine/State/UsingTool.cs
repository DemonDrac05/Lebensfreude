using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsingTool : PlayerState
{
    public Item item;
    public new Player player;
    public ToolUsedManager hoeTool;
    public UsingTool(Player player, PlayerStateMachine playerStateMachine) : base(player, playerStateMachine)
    {
        this.player = player;
        item = FindObjectOfType<Item>();
        hoeTool = FindObjectOfType<ToolUsedManager>();
    }

    public override void EnterState()
    {
        item = InventoryManager.Instance.GetSelectedItem(false);
        hoeTool.usingTime = hoeTool.usingDuration;
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        if (item.itemType == ItemType.Tool)
        {
            hoeTool.usingTime -= Time.deltaTime;
            player.movementInput = Vector2.zero;
            string animtion = item.actionType.ToString() + player.FacingDirection.ToString();
            player.animator.Play(animtion);

            if (hoeTool.usingTime <= 0f)
                player.stateMachine.ChangeState(player.movementState);
        }

        else player.stateMachine.ChangeState(player.movementState);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}

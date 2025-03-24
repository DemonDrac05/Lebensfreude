using UnityEngine;

public class UsingTool : PlayerState
{
    public new Player player;

    public Tool toolItem;
    public ToolUsedManager tool;

    public UsingTool(Player player, PlayerStateMachine playerStateMachine) : base(player, playerStateMachine)
    {
        this.player = player;
    }

    public override void EnterState()
    {
        Initialize();
    }

    public override void FrameUpdate()
    {
        tool.usingTime -= Time.deltaTime;
        player.movementInput = Vector2.zero;
        string animtion = toolItem.actionType.ToString() + player.FacingDirection.ToString();
        player.animator.Play(animtion);

        if (tool.usingTime <= 0f)
            player.stateMachine.ChangeState(player.movementState);
    }

    private void Initialize()
    {
        toolItem = InventoryManager.Instance.GetSelectedItem<Tool>(false);
        tool = ToolUsedManager.Instance;

        tool.usingTime = tool.usingDuration;
    }
}

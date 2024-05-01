using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player;

public class MovementState : PlayerState
{
    public Item item;
    public new Player player;
    public PlantingManager plantingManager;

    public MovementState(Player player, PlayerStateMachine playerStateMachine) : base(player, playerStateMachine)
    {
        this.player = player;
        item = FindObjectOfType<Item>();
        plantingManager = FindObjectOfType<PlantingManager>();
    }

    public override void FrameUpdate()
    {
        player.movementInput.x = Input.GetAxisRaw("Horizontal");
        player.movementInput.y = Input.GetAxisRaw("Vertical");
        player.rb2d.MovePosition(player.rb2d.position + player.movementInput.normalized * player.movementSpeed * Time.fixedDeltaTime);

        if (player.movementInput != Vector2.zero)
        {
            CheckFacialDirection();
            string animation = "Move" + player.FacingDirection.ToString();

            if (animation != null) player.animator.Play(animation);
        }
        else
        {
            string animation = "Idle" + player.FacingDirection.ToString();

            if (animation != null) player.animator.Play(animation);
        }

        InventorySlot slot = InventoryManager.Instance.InventorySlots[InventoryManager.Instance.selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

        if (Input.GetMouseButtonDown(0) && itemInSlot != null)
        {
            UpdateDirectionWithHitBox();
            player.stateMachine.ChangeState(player.waterState);
        }
    }

    public void CheckFacialDirection()
    {
        if (Mathf.Abs(player.movementInput.x) > Mathf.Abs(player.movementInput.y))
            player.FacingDirection = (player.movementInput.x > 0) ? FacialDirection.Right : FacialDirection.Left;
        else
            player.FacingDirection = (player.movementInput.y > 0) ? FacialDirection.Up : FacialDirection.Down;
    }

    public void UpdateDirectionWithHitBox()
    {
        if (player.mousePosition.x == player.Position.x + 1.5f)
            player.FacingDirection = Player.FacialDirection.Right;
        else if (player.mousePosition.x == player.Position.x - 0.5f)
            player.FacingDirection = Player.FacialDirection.Left;
        else if (player.mousePosition.y == player.Position.y + 0.5f)
            player.FacingDirection = Player.FacialDirection.Up;
        else if (player.mousePosition.y == player.Position.y - 0.5f)
            player.FacingDirection = Player.FacialDirection.Down;
    }
}

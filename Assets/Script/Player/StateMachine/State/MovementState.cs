using UnityEngine;
using static Player;

public class MovementState : PlayerState
{
    public new Player player;

    public MovementState(Player player, PlayerStateMachine playerStateMachine) : base(player, playerStateMachine)
    {
        this.player = player;
    }

    public override void FrameUpdate()
    {
        if (InputBlocker.IsBlocked) return; // đang ngủ/overlay -> đứng yên, không nhận input di chuyển

        bool freezeAnimation = !InventoryManager.Instance.toolbar.activeSelf;
        if (!freezeAnimation)
        {
            player.movementInput.x = Input.GetAxisRaw("Horizontal");
            player.movementInput.y = Input.GetAxisRaw("Vertical");
            float staminaMult = StaminaManager.Instance != null ? StaminaManager.Instance.MoveSpeedMultiplier : 1f;

            // ── Di chuyển CÓ CHẶN Ô (tile-based) ─────────────────────────
            // Kiểm tra theo TỪNG TRỤC: ô đích trên trục X bị chặn thì bỏ X,
            // trục Y bị chặn thì bỏ Y -> player vẫn TRƯỢT DỌC tường mượt.
            // Chỉ là HashSet lookup O(1) -> không physics, không drop FPS.
            Vector2 delta = player.movementInput.normalized * player.movementSpeed * staminaMult * Time.fixedDeltaTime;
            Vector2 pos = player.rb2d.position;
            Vector2 dest = pos;
            if (delta.x != 0f && !WorldBlocking.IsBlockedWorld(new Vector3(pos.x + delta.x, pos.y, 0f))) dest.x = pos.x + delta.x;
            if (delta.y != 0f && !WorldBlocking.IsBlockedWorld(new Vector3(dest.x, pos.y + delta.y, 0f))) dest.y = pos.y + delta.y;
            player.rb2d.MovePosition(dest);

            UsingToolState();
        }
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
    }

    private void UsingToolState()
    {
        ToolbarSlot slot = InventoryManager.Instance.ToolbarSlots[InventoryManager.Instance.selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

        if (Input.GetMouseButtonDown(0) && itemInSlot != null)
        {
            bool allowToChangeState = ToolUsedManager.Instance.isReadyToUse;
            if (allowToChangeState)
            {
                UpdateDirectionWithHitBox();
                if (itemInSlot.GetItem<Tool>() != null)
                {
                    player.stateMachine.ChangeState(player.waterState);
                }
            }
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

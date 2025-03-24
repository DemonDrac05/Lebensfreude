using UnityEngine;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb2d;
    public Animator animator;
    public PlayerController playerController;

    public Vector2 movementInput;
    public float movementSpeed = 5f;

    #region State Machine
    public PlayerStateMachine stateMachine;
    public MovementState movementState;
    public UsingTool waterState;
    #endregion

    #region Direction
    public enum FacialDirection { None, Right, Left, Up, Down }
    public FacialDirection FacingDirection { get; set; } = FacialDirection.None;

    public Vector3 mousePosition = new();
    public Vector3Int Position = new();
    #endregion

    private void Awake()
    {
        stateMachine = new PlayerStateMachine();
        movementState = new MovementState(this, stateMachine);
        waterState = new UsingTool(this, stateMachine);
    }

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();

        stateMachine.Initialize(movementState);
    }

    private void FixedUpdate()
    {
        this.mousePosition = playerController.mousePosUpdate;
        this.Position = playerController.playerPosUpdate;

        stateMachine.playerState.PhysicsUpdate();
    }
    private void Update()
    {
        stateMachine.playerState.FrameUpdate();
    }
}

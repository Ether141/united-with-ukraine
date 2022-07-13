using System;
using UnityEngine;

[GameManagerMember]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IMovementController
{
    [Header("Movement")]
    public bool canMove = true;
    public bool canCrouch = true;
    public float movementSpeed = 9f;
    public float crouchSpeed = 6f;
    public float ladderMovementSpeed = 6f;
    public float yVelLimit = 10f;

    [Header("Jumping")]
    public bool canJump = true;
    public float jumpForce = 8f;
    public float maxJumpTime = 1f;
    public float topEdgeMaxOffset = 0.15f;
    [Range(0f, 1f)] public float inAirControl = 0.5f;

    [Header("Dashing")]
    public bool canDash = true;
    public float dashForce = 50f;
    public float downDashForce = 8f;
    public float dashForceResetingSpeed = 10f;

    [Header("Sticky wall")]
    public bool canStickToWall = true;
    public float stickyWallFactor = 0.1f;
    public float jumpForceOnStickyWall = 13f;
    public float stickyWallBehindRayLength = 1f;

    [Header("Step")]
    public float maxStepHeight = 0.4f;
    public float stepCheckIteration = 0.01f;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask ladderLayer;

    private Vector2 movementVelocity;
    private Vector2 ladderVelocity;
    private Vector2 startColSize;
    private Vector2 startColOffset;
    private bool jumpWasCalled = false;
    private float jumpTimeCounter = 0f;
    private bool canAddJumpVelocity = true;
    private Transform ladder;
    private Bounds ladderBounds;
    private Vector2 dashPendingForce = Vector2.zero;
    private bool wasStickedToWall = false;
    private bool isStickyWallBehind = false;

    private bool forceMovement = false;
    private float forcedMovementSpeed = 2f;
    private Vector2 forcedPosition;

    private Rigidbody2D rb2d;
    private BoxCollider2D col;

    private const float CheckGroundRayLength = 0.75f;

    public float HorizontalMovement { get; private set; } = 0f;
    public float VerticalMovement { get; private set; } = 0f;
    public float CurrentSpeed => canMove ? (IsCrouching ? crouchSpeed : movementSpeed * (!IsGrounded && !IsOnLadder && jumpWasCalled ? inAirControl : 1f)) : 0f;
    public bool IsMoving => Mathf.Abs(HorizontalMovement) + (IsOnLadder ? Mathf.Abs(VerticalMovement) : 0f) > 0.1f && canMove;
    public bool IsCrouching { get; private set; } = false;
    public bool IsGrounded { get; private set; } = true;
    public bool IsOnLadder { get; private set; } = false;
    public bool IsClimbingLadder { get; private set; } = false;
    public bool IsDashing { get; private set; } = false;
    public bool IsOnStickyWall { get; private set; } = false;
    public Bounds StartCharacterBounds { get; private set; }
    public Bounds CurrentCharacterBounds => col.bounds;
    public bool IsCrouchPressed => Input.GetButton("Crouch");
    public float GroundStickYPoint => transform.position.y - StartCharacterBounds.extents.y + col.offset.y;
    public Vector2 CurrentVelocity => rb2d.velocity;
    public bool IsOnOneWayPlatform
    {
        get
        {
            RaycastHit2D leftHit = Physics2D.Raycast(new Vector2(transform.position.x - CurrentCharacterBounds.extents.x, GroundStickYPoint), -Vector2.up, CheckGroundRayLength, groundLayer);
            RaycastHit2D centerHit = Physics2D.Raycast(new Vector2(transform.position.x, GroundStickYPoint), -Vector2.up, CheckGroundRayLength, groundLayer);
            RaycastHit2D rightHit = Physics2D.Raycast(new Vector2(transform.position.x + CurrentCharacterBounds.extents.x, GroundStickYPoint), -Vector2.up, CheckGroundRayLength, groundLayer);
            return leftHit.transform?.GetComponent<OneWayPlatform>() != null || centerHit.transform?.GetComponent<OneWayPlatform>() != null || rightHit.transform?.GetComponent<OneWayPlatform>() != null;
        }
    }
    public bool IsSomethingAbove
    {
        get
        {
            float yPos = transform.position.y + StartCharacterBounds.extents.y;
            const float dis = 0.5f;

            RaycastHit2D leftHit = Physics2D.Raycast(new Vector2(transform.position.x - CurrentCharacterBounds.extents.x, yPos), -Vector2.up, dis, groundLayer);
            RaycastHit2D centerHit = Physics2D.Raycast(new Vector2(transform.position.x, yPos), -Vector2.up, dis, groundLayer);
            RaycastHit2D rightHit = Physics2D.Raycast(new Vector2(transform.position.x + CurrentCharacterBounds.extents.x, yPos), -Vector2.up, dis, groundLayer);

            return leftHit.transform != null || centerHit.transform != null || rightHit.transform != null;
        }
    }

    public event Action OnLand;
    private bool onLandWasInvoked = false;

    private void Awake() => GameManager.PlayerReference = gameObject;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        startColSize = col.size;
        startColOffset = col.offset;
        StartCharacterBounds = CurrentCharacterBounds;
    }

    private void Update()
    {
        CollectInput();
        CheckGroundedState();
        StickyWallClimber();
        JumpController();
        CrouchController();
        StepHelper();
        Dashing();
        ForcePosition();

        if (rb2d.velocity.y <= yVelLimit)
            rb2d.velocity = new Vector2(rb2d.velocity.x, yVelLimit);
    }

    private void FixedUpdate()
    {
        PlayerMovement();
        LadderMovement();
    }

    private void PlayerMovement()
    {
        movementVelocity = new Vector2(HorizontalMovement * CurrentSpeed, 0f);
        Vector2 resultVelocity;

        if (!IsOnLadder)
        {
            resultVelocity = new Vector2(movementVelocity.x, rb2d.velocity.y) + dashPendingForce;
        }
        else
        {
            resultVelocity = ladderVelocity;
        }

        rb2d.velocity = resultVelocity;
    }

    private void CollectInput()
    {
        HorizontalMovement = Input.GetAxisRaw("Horizontal");
        VerticalMovement = Input.GetAxisRaw("Vertical");

        if (IsMoving)
            forceMovement = false;
    }

    private void JumpController()
    {
        if (Input.GetButtonDown("Jump") && !IsCrouching && canJump && (IsGrounded || isStickyWallBehind) && !IsOnStickyWall && canMove)
        {
            jumpWasCalled = true;
            GetComponent<PlayerAnimator>().CallJump();

            if (!isStickyWallBehind)
                rb2d.velocity = Vector2.up * jumpForce;
            else
                rb2d.velocity += Vector2.up * jumpForceOnStickyWall + (Vector2)transform.right * 33f;
        }

        if (Input.GetButton("Jump") && jumpWasCalled && canAddJumpVelocity && jumpTimeCounter < maxJumpTime && !IsOnStickyWall && !IsSomethingAbove)
        {
            jumpTimeCounter += Time.deltaTime;
            rb2d.velocity = Vector2.up * jumpForce;
        }
        else
        {
            jumpTimeCounter = 0f;
            canAddJumpVelocity = false;
            rb2d.gravityScale = IsOnStickyWall ? stickyWallFactor : (IsOnLadder ? 0f : 3f);
        }

        if (Input.GetButtonUp("Jump"))
            canAddJumpVelocity = false;

        UpEdgeCollisionHelper();
    }

    private void CheckGroundedState()
    {
        RaycastHit2D leftHit = Physics2D.Raycast(new Vector2(transform.position.x - CurrentCharacterBounds.extents.x + 0.1f, GroundStickYPoint), -Vector2.up, CheckGroundRayLength, groundLayer);
        RaycastHit2D centerHit = Physics2D.Raycast(new Vector2(transform.position.x, GroundStickYPoint), -Vector2.up, CheckGroundRayLength, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(new Vector2(transform.position.x + CurrentCharacterBounds.extents.x - 0.1f, GroundStickYPoint), -Vector2.up, CheckGroundRayLength, groundLayer);

        Debug.DrawLine(new Vector2(transform.position.x - CurrentCharacterBounds.extents.x + 0.1f, GroundStickYPoint), new Vector2(transform.position.x - CurrentCharacterBounds.extents.x + 0.1f, GroundStickYPoint) + (Vector2)(-Vector2.up * CheckGroundRayLength), Color.green);
        Debug.DrawLine(new Vector2(transform.position.x, GroundStickYPoint), new Vector2(transform.position.x, GroundStickYPoint) + (-Vector2.up * CheckGroundRayLength), Color.green);
        Debug.DrawLine(new Vector2(transform.position.x + CurrentCharacterBounds.extents.x - 0.1f, GroundStickYPoint), new Vector2(transform.position.x + CurrentCharacterBounds.extents.x - 0.1f, GroundStickYPoint) + (Vector2)(-Vector2.up * CheckGroundRayLength), Color.green);

        bool isCollision = leftHit.transform != null || centerHit.transform != null || rightHit.transform != null;
        bool prevState = IsGrounded;

        IsGrounded = isCollision && rb2d.velocity.y <= 0f;

        if (!prevState && IsGrounded && !onLandWasInvoked)
        {
            onLandWasInvoked = true;
            OnLand?.Invoke();
        }

        if (IsGrounded)
        {
            onLandWasInvoked = false;
            canAddJumpVelocity = true;
            jumpWasCalled = false;
        }
    }

    private void Dashing()
    {
        dashPendingForce = Vector2.Lerp(dashPendingForce, Vector2.zero, Time.deltaTime * dashForceResetingSpeed);

        if (IsGrounded)
            IsDashing = false;

        if (!IsDashing && !IsGrounded && canDash && canMove && GameManager.PlayerStats.IsDashAvailable)
        {
            if (Input.GetButtonDown("Dash"))
            {
                IsDashing = true;
                GameManager.PlayerStats.UseDash();

                float xForce = HorizontalMovement != 0f ? HorizontalMovement : 0f;
                float yForce = xForce == 0f || VerticalMovement < 0f ? -1f : 0f;
                float force = yForce == -1f ? downDashForce : dashForce;

                Vector2 dir = new Vector2(xForce, yForce);
                dashPendingForce = dir * force;
            }
        }
    }

    private void StepHelper()
    {
        float xPos = transform.position.x + (transform.right * StartCharacterBounds.extents.x).x;
        float yPos = transform.position.y - StartCharacterBounds.extents.y;
        const float dis = 0.25f;

        Debug.DrawRay(new Vector3(xPos, yPos + maxStepHeight, dis), transform.right, Color.red);

        if (!IsMoving || !IsGrounded)
            return;

        if (Physics2D.Raycast(new Vector2(xPos, yPos + stepCheckIteration), transform.right, dis, groundLayer))
        {
            if (Physics2D.Raycast(new Vector2(xPos, yPos + maxStepHeight), transform.right, dis, groundLayer))
                return;

            float currentYPos = yPos + stepCheckIteration;

            while (Physics2D.Raycast(new Vector2(xPos, currentYPos), transform.right, dis, groundLayer))
                currentYPos += stepCheckIteration;

            float offset = Mathf.Abs(yPos - currentYPos);
            transform.position = transform.position + new Vector3(transform.right.x * StartCharacterBounds.extents.x, offset, 0f);
        }
    }

    private void UpEdgeCollisionHelper()
    {
        float yPos = transform.position.y + StartCharacterBounds.extents.y;
        const float dis = 0.5f;

        Debug.DrawLine(new Vector2(transform.position.x + CurrentCharacterBounds.extents.x, yPos), new Vector2(transform.position.x + CurrentCharacterBounds.extents.x, yPos) + (Vector2.up * dis), Color.blue);
        Debug.DrawLine(new Vector2(transform.position.x + CurrentCharacterBounds.extents.x - topEdgeMaxOffset, yPos), new Vector2(transform.position.x + CurrentCharacterBounds.extents.x - topEdgeMaxOffset, yPos) + (Vector2.up * dis), Color.blue);

        Debug.DrawLine(new Vector2(transform.position.x - CurrentCharacterBounds.extents.x, yPos), new Vector2(transform.position.x - CurrentCharacterBounds.extents.x, yPos) + (Vector2.up * dis), Color.blue);
        Debug.DrawLine(new Vector2(transform.position.x - CurrentCharacterBounds.extents.x + topEdgeMaxOffset, yPos), new Vector2(transform.position.x - CurrentCharacterBounds.extents.x + topEdgeMaxOffset, yPos) + (Vector2.up * dis), Color.blue);

        if (IsGrounded)
            return;

        Transform rightHitA = Physics2D.Raycast(new Vector2(transform.position.x + CurrentCharacterBounds.extents.x, yPos), Vector2.up, dis, groundLayer).transform;
        Transform rightHitB = Physics2D.Raycast(new Vector2(transform.position.x + CurrentCharacterBounds.extents.x - topEdgeMaxOffset, yPos), Vector2.up, dis, groundLayer).transform;

        Transform leftHitA = Physics2D.Raycast(new Vector2(transform.position.x - CurrentCharacterBounds.extents.x, yPos), Vector2.up, dis, groundLayer).transform;
        Transform leftHitB = Physics2D.Raycast(new Vector2(transform.position.x - CurrentCharacterBounds.extents.x + topEdgeMaxOffset, yPos), Vector2.up, dis, groundLayer).transform;

        if (rightHitA != null && rightHitB == null && leftHitA == null && leftHitB == null)
            transform.position += (Vector3)(-Vector2.right * topEdgeMaxOffset);
        else if (leftHitA != null && leftHitB == null && rightHitA == null && rightHitB == null)
            transform.position += (Vector3)(Vector2.right * topEdgeMaxOffset);
    }

    private void CrouchController()
    {
        if (IsCrouchPressed && IsGrounded && canCrouch && canMove)
        {
            col.size = new Vector2(col.size.x, startColSize.y / 2f);
            col.offset = new Vector2(startColOffset.x, startColOffset.y - col.size.y / 2f);
            IsCrouching = true;
        }
        else if (!IsSomethingAbove)
        {
            col.offset = startColOffset;
            col.size = startColSize;
            IsCrouching = false;
        }
    }

    private void StickyWallClimber()
    {
        if (!canStickToWall)
            return;

        float xPos = transform.position.x + (transform.right * StartCharacterBounds.extents.x).x;
        float dis = 0.2f;

        Transform hitA = Physics2D.Raycast(new Vector2(xPos, GroundStickYPoint + 0.25f), transform.right, dis, wallLayer).transform;
        Transform hitB = Physics2D.Raycast(new Vector2(xPos, GroundStickYPoint + CurrentCharacterBounds.size.y - 0.4f), transform.right, dis, wallLayer).transform;

        IsOnStickyWall = (hitA != null && hitA.CompareTag("StickyWall")) || (hitB != null && hitB.CompareTag("StickyWall")) && !IsGrounded;

        if (IsOnStickyWall)
        {
            if (!wasStickedToWall)
            {
                rb2d.velocity = Vector2.zero;
                jumpWasCalled = false;
                wasStickedToWall = true;
            }
        }
        else
        {
            wasStickedToWall = false;
        }

        xPos = transform.position.x + (-transform.right * StartCharacterBounds.extents.x).x;
        hitA = Physics2D.Raycast(new Vector2(xPos, GroundStickYPoint + 0.25f), -transform.right, stickyWallBehindRayLength, wallLayer).transform;
        hitB = Physics2D.Raycast(new Vector2(xPos, GroundStickYPoint + CurrentCharacterBounds.size.y - 0.4f), -transform.right, stickyWallBehindRayLength, wallLayer).transform;

        Debug.DrawLine(new Vector2(xPos, GroundStickYPoint + 0.25f), new Vector2(xPos, GroundStickYPoint + 0.25f) + (Vector2)(-transform.right * stickyWallBehindRayLength), Color.black);
        Debug.DrawLine(new Vector2(xPos, GroundStickYPoint + CurrentCharacterBounds.size.y - 0.4f), new Vector2(xPos, GroundStickYPoint + CurrentCharacterBounds.size.y - 0.4f) + (Vector2)(-transform.right * stickyWallBehindRayLength), Color.black);

        isStickyWallBehind = (hitA != null && hitA.CompareTag("StickyWall") || (hitB != null && hitB.CompareTag("StickyWall"))) && !IsGrounded;
    }

    private void LadderMovement()
    {
        IsClimbingLadder = (Mathf.Abs(VerticalMovement) > 0f || Mathf.Abs(HorizontalMovement) > 0f) && IsOnLadder;
        bool isOnTop = ladder != null && (transform.position.y - CurrentCharacterBounds.extents.y) > (ladder.position.y + ladderBounds.extents.y - 0.25f);
        ladderVelocity = IsClimbingLadder ?
                         new Vector2(HorizontalMovement * CurrentSpeed, VerticalMovement * ladderMovementSpeed * (VerticalMovement > 0f && isOnTop ? 0f : 1f)) :
                         new Vector2(0f, 0f);
    }

    private void ForcePosition()
    {
        if (forceMovement)
            transform.position = Vector3.Lerp(transform.position, forcedPosition, Time.deltaTime * forcedMovementSpeed);
    }

    public void Death()
    {
        canMove = false;
        rb2d.velocity = Vector2.up * jumpForce;
        jumpWasCalled = true;
        col.enabled = false;
    }

    public void MoveTo(Vector2 pos, float speed)
    {
        forcedPosition = pos;
        forcedMovementSpeed = speed;
        forceMovement = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (ladderLayer == (ladderLayer | (1 << collision.gameObject.layer)) && !IsDashing)
        {
            IsOnLadder = true;
            ladder = collision.transform;
            ladderBounds = collision.bounds;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (ladderLayer == (ladderLayer | (1 << collision.gameObject.layer)))
        {
            IsOnLadder = false;
            IsClimbingLadder = false;
        }
    }
}

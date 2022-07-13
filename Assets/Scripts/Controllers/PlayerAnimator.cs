using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite crouchSprite;

    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        FlipPlayer();
        Crouching();
        HandleAnimator();
    }

    public void CallJump() => anim.SetTrigger("jump");

    private void HandleAnimator()
    {
        anim.SetFloat("x", playerController.IsMoving ? Mathf.Abs(playerController.HorizontalMovement) : 0f);
        anim.SetBool("isGrounded", playerController.IsGrounded);
    }

    private void Crouching() => spriteRenderer.sprite = playerController.IsCrouching ? crouchSprite : normalSprite;

    private void FlipPlayer()
    {
        if(playerController.IsMoving)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, playerController.HorizontalMovement < 0f ? 180f : 0f, transform.eulerAngles.z);
    }
}

using UnityEngine;

public class BreakableWallTrigger : MonoBehaviour
{
    [SerializeField] private BreakableWall.WallHitSide triggerSide;

    private BreakableWall associatedWall;

    private void Start() => associatedWall = transform.parent.GetComponent<BreakableWall>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && GameManager.PlayerController.IsDashing && GameManager.PlayerController.CurrentVelocity.x >= BreakableWall.RequiredXVelocity)
        {
            associatedWall.BreakWall(triggerSide);
        }
    }
}

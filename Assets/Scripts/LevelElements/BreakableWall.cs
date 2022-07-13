using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    public const float RequiredXVelocity = 18f;

    public void BreakWall(WallHitSide hitSide)
    {
        Destroy(gameObject);
    }

    public enum WallHitSide
    {
        Right,
        Left
    }
}

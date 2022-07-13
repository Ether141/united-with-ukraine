using UnityEngine;

public class StraightPath : MonoBehaviour
{
    public float length = 10f;
    public bool hasAnyAssociatedGhost = false;
    public Vector2 StartPosition => (Vector2)transform.position - (Vector2.right * (length / 2));
    public Vector2 EndPosition => (Vector2)transform.position + (Vector2.right * (length / 2));

    public Vector2 GetPositionAtTime(float time)
    {
        time = Mathf.Clamp01(time);
        return Vector2.Lerp(StartPosition, EndPosition, time);
    }

    public bool IsOnPath(Vector2 pos) => pos.x >= StartPosition.x && pos.x <= EndPosition.x;

    public bool IsAtStart(Vector2 pos) => pos.x == StartPosition.x;

    public bool IsAtEnd(Vector2 pos) => pos.x == EndPosition.x;

    public Vector2 ClosestPoint(Vector2 pos) => Vector2.Distance(pos, StartPosition) < Vector2.Distance(pos, EndPosition) ? StartPosition : EndPosition;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(StartPosition, EndPosition);
    }
}

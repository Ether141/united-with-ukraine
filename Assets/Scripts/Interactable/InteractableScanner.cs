using UnityEngine;

[GameManagerMember]
public class InteractableScanner : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Vector2 overlapBoxSize;
    [SerializeField] private Vector2 overlapBoxOffset;

    public GameObject Result { get; private set; }
    public bool IsLookingAtAnything => Result != null;

    private Vector2 OverlapBoxCenter => new Vector2(transform.position.x, transform.position.y) + (overlapBoxOffset.x * (Vector2)transform.right) + (overlapBoxOffset.y * (Vector2)transform.up);

    private void Update()
    {
        Collider2D col = Physics2D.OverlapBox(OverlapBoxCenter, overlapBoxSize, 0f, interactableLayer);

        if (col != null)
            Result = col.gameObject;
        else
            Result = null;
    }

    public bool IsLookingAt(GameObject toCheck) => IsLookingAtAnything && Result == toCheck;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(OverlapBoxCenter, new Vector3(overlapBoxSize.x, overlapBoxSize.y, 0f));
    }
}

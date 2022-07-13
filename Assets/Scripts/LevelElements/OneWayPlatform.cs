using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OneWayPlatform : MonoBehaviour
{
    private Collider2D col;

    private void Start() => col = GetComponent<Collider2D>();

    private void Update()
    {
        if ((GameManager.PlayerController.IsDashing && GameManager.PlayerController.CurrentVelocity.y < -15f) || GameManager.PlayerController.GroundStickYPoint < transform.position.y)
        {
            col.enabled = false;
        }
        else
        {      
            col.enabled = true;
        }
    }
}

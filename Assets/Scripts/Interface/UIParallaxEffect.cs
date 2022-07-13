using UnityEngine;

public class UIParallaxEffect : MonoBehaviour
{
    [SerializeField] private float moveModifier = 2f;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        Vector3 pos = -Camera.main.ScreenToViewportPoint(Input.mousePosition);
        pos.z = 0;
        gameObject.transform.position = pos;
        transform.position = new Vector3(startPos.x + (pos.x * moveModifier), startPos.y + (pos.y * moveModifier), 0);
    }
}

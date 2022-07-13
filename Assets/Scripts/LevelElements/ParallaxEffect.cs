using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [SerializeField] private GameObject cam;
    [SerializeField] private float parallaxEffect;
    [SerializeField] private bool followCameraY = true;

    private float length;
    private float startpos;
    private float startposY;

    private void Start()
    {
        startpos = transform.position.x;
        startposY = transform.position.y;
        length = GetComponent<SpriteRenderer>() ? GetComponent<SpriteRenderer>().bounds.size.x : 0f; 
    }

    private void FixedUpdate()
    {
        float tmp = cam.transform.position.x * (1 - parallaxEffect);
        float dist = cam.transform.position.x * parallaxEffect;
        float distY = cam.transform.position.y * parallaxEffect;

        transform.position = new Vector3(startpos + dist, followCameraY ? startposY + distY : transform.position.y, transform.position.z);

        if (tmp > startpos + length)
            startpos += length;
        else if (tmp < startpos - length)
            startpos -= length;
    }
}
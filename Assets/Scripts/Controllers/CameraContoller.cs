using UnityEngine;

[GameManagerMember]
public class CameraContoller : MonoBehaviour
{
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private float targetRequiredShift = 0.5f;
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float fovChangeSpeed = 10f;

    public Vector3 TargetPosition { get; private set; }

    private Vector2 TargetShift => new Vector2(Mathf.Abs(target.position.x - transform.position.x), Mathf.Abs(target.position.y - (transform.position.y - offset.y)));
    private Camera cam;

    public float StartFov { get; private set; }
    private float targetFov;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        GameManager.MainCamera = cam;
        StartFov = targetFov = cam.orthographicSize;
    }

    private void Update()
    {
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetFov, Time.deltaTime * fovChangeSpeed);
    }

    private void FixedUpdate()
    {
        if (TargetShift.x > targetRequiredShift || TargetShift.y > targetRequiredShift)
            TargetPosition = new Vector3(target.position.x, target.position.y, -1f);

        transform.position = Vector3.Lerp(transform.position, TargetPosition + offset, Time.deltaTime * speed);
    }

    public void ChangeFOV(float fov, float speed)
    {
        targetFov = fov;
        fovChangeSpeed = speed;
    }

    public void ResetFOV(float speed)
    {
        targetFov = StartFov;
        fovChangeSpeed = speed;
    }

    public void ForceFocus() => TargetPosition = new Vector3(target.position.x, target.position.y, -1f);
}

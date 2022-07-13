using UnityEngine;
using UnityEngine.UI;

public class CursorSprite : MonoBehaviour
{
    [SerializeField] private Sprite normalCursor;
    [SerializeField] private Sprite clickCursor;

    private RectTransform rect;
    private Image image;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector3 pos = (Input.mousePosition * (1920f / Screen.width)) + new Vector3(25f, -25f, 0f);
        rect.anchoredPosition = pos;
        image.sprite = Input.GetMouseButton(0) ? clickCursor : normalCursor;
        image.color = new Color(1f, 1f, 1f, Cursor.lockState == CursorLockMode.Locked ? 0f : 1f);
    }
}

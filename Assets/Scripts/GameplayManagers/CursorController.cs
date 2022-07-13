using UnityEngine;

public static class CursorController
{
    public static void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }

    public static void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

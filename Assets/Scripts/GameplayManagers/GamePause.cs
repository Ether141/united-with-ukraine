using System;
using UnityEngine;

public class GamePause : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseUI;

    public bool IsGamePaused { get; private set; } = false;
    public bool CanPause => !IsGamePaused;

    public event Action<bool> OnChangedPauseState;

    private void Start() => CursorController.HideCursor();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsGamePaused)
                ResumeGame();
            else if (CanPause)
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (!CanPause)
            return;

        IsGamePaused = true;
        Time.timeScale = 0f;
        pauseUI.SetActive(true);
        CursorController.ShowCursor();

        OnChangedPauseState?.Invoke(true);
    }

    public void ResumeGame()
    {
        if (!IsGamePaused)
            return;

        IsGamePaused = false;
        Time.timeScale = 1f;
        pauseUI.SetActive(false);
        CursorController.HideCursor();

        OnChangedPauseState?.Invoke(false);
    }
}

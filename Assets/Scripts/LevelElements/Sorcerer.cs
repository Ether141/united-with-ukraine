using UnityEngine;
using UnityEngine.SceneManagement;

public class Sorcerer : Interactable
{
    [SerializeField] private GameObject winUI;
    [SerializeField] private int thisScene = 1;
    [SerializeField] private int nextLevel = 2;
    [SerializeField] private int nextScene = 2;

    protected override bool AdditionalInteractConditions => GameManager.LevelManager.GhostsSpawner.AllGhostsSaved;

    public override void Inspect()
    {
        base.Inspect();
        winUI.SetActive(true);
        CursorController.ShowCursor();
        GameManager.PlayerController.canMove = false;
    }

    public void LoadNextScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextScene);
        SaveSystem.AddEntry("unlockedLevel", nextLevel);
    }
    
    public void ReloadScene()
    {
        SceneManager.LoadScene(thisScene);
    }

    private void OnValidate()
    {
        label = "Finish the rite";
        needToHoldKey = true;
    }
}

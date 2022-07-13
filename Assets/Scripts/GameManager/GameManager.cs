using UnityEngine;

public static class GameManager
{
    public static CameraContoller CameraContoller { get; private set; }
    public static SoundsProvider SoundsProvider { get; private set; }
    public static PlayerController PlayerController { get; private set; }
    public static InteractableScanner PlayerInteractableScanner { get; private set; }
    public static GameplayUIManager GameplayUIManager { get; private set; }
    public static PlayerStats PlayerStats { get; private set; }
    public static ResourcesManager ResourcesManager { get; private set; }
    public static DialogueDisplayer DialogueDisplayer { get; private set; }
    public static LevelManager LevelManager { get; private set; }
    public static PlotItems PlotItems { get; private set; }

    public static GameObject PlayerReference { get; set; }
    public static Camera MainCamera { get; set; }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

public class MapOfLevels : MonoBehaviour
{
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private Image marker2;
    [SerializeField] private Animator anim;
    [SerializeField] private Text label;

    private bool level2Unlocked = false;
    private int selectedLevel = 0;

    private void Awake() => SaveSystem.LoadFromFile();

    private void Start()
    {
        LoadSavedValues();
    }

    private void LoadSavedValues()
    {
        level2Unlocked = SaveSystem.EntryExsists("unlockedLevel") ? SaveSystem.LoadEntry<int>("unlockedLevel") >= 2 : false; 

        if (level2Unlocked)
        {
            marker2.color = new Color(1f, 1f, 1f, 1f);
            marker2.GetComponent<Button>().interactable = true;
            marker2.GetComponent<Animator>().enabled = true;
        }
    }

    public void StartLevel()
    {
        if (selectedLevel > 0)
        {
            mainMenu.ActivateScreen(3);
            mainMenu.targetVolume = 0f;
            this.WaitAndDo(() => SceneManager.LoadScene(selectedLevel), 2f);
            CursorController.HideCursor();
        }
    }

    public void ShowPlayUI(int level)
    {
        selectedLevel = level;
        anim.SetBool("isShown", true);

        label.text = level switch
        {
            2 => "Kviv",
            _ => "Lviv"
        };
    }
}

using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

[GameManagerMember]
public class ResourcesManager : MonoBehaviour
{
    [SerializeField] private Skills[] availableSkills;
    [SerializeField] private int stepLevel = 20;

    [Header("UI")]
    [SerializeField] private GameObject ui;

    public int StepsCount => availableSkills.Length;
    public int UnlockedStepsCount { get; private set; } = 0;
    public int CurrentProgress => currentProgress;
    public Skills[] UnlockedSkills => availableSkills.Take(UnlockedStepsCount).ToArray();

    private int currentProgress = 0;
    private int MaxProgress => StepsCount * stepLevel;

    private GameObject skillLabelTemplate;
    private Image progressBarFill;

    private Text[] skillLabels;

    private void Start()
    {
        progressBarFill = ui.transform.GetChildWithName("ProgressBarFill").GetComponent<Image>();
        skillLabelTemplate = ui.transform.GetChildWithName("SkillLabelTemplate").gameObject;
        InitializeUI();
    }

    public void AddProgress(int progress)
    {
        if (currentProgress + progress >= MaxProgress)
            currentProgress = MaxProgress;
        else
            currentProgress += progress;

        UnlockedStepsCount = Mathf.FloorToInt(currentProgress / stepLevel);
        GameManager.GameplayUIManager.ShowGameplayUI();
        UpdateUI();

        if (HasUnlockedSkill(Skills.Dash))
            GameManager.PlayerController.canDash = true;

        if (HasUnlockedSkill(Skills.HigherJump))
            GameManager.PlayerController.jumpForce *= 1.25f;
    }

    private void UpdateUI()
    {
        progressBarFill.fillAmount = (float)currentProgress / MaxProgress;

        for (int i = 0; i < UnlockedStepsCount; i++)
            skillLabels[i].text = SkillToString(availableSkills[i]);
    }

    private void InitializeUI()
    {
        skillLabels = new Text[StepsCount];
        float offset = ui.GetComponent<RectTransform>().sizeDelta.y / StepsCount;

        for (int i = 0; i < StepsCount; i++)
        {
            skillLabels[i] = Instantiate(skillLabelTemplate, ui.transform).GetComponent<Text>();
            RectTransform rect = skillLabels[i].GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, offset + (offset * i) + (rect.sizeDelta.y / 2));
            skillLabels[i].gameObject.SetActive(true);
        }
    }

    public bool HasUnlockedSkill(Skills skill) => UnlockedSkills.Any(s => s == skill);

    public static string SkillToString(Skills skill)
    {
        return skill switch
        {
            Skills.Dash => "Dash",
            Skills.GhostsLowerHealth => "The ghosts are weakened",
            Skills.HigherDamage => "Higher dealt damage",
            Skills.HigherJump => "Higher jump",
            _ => "Unknown skill",
        };
    }
}

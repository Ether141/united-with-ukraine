using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

[GameManagerMember]
public class GameplayUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject keyPressUI;
    [SerializeField] private GameObject gameplayUI;

    #region Key press
    private MonoBehaviour keyPressOwner;
    private Text keyPressLabel;
    private Image keyHoldFill;
    private bool keyPressHoldMode = false;
    private event Action KeyHoldingFinished; 
    #endregion

    private float keyHoldFillSpeed = 2.25f;
    private float keyHoldTimer = 0f;

    private IEnumerator gameplayUICoroutine;
    private GameObject ghostsIndicator;
    private GameObject specialGhostsIndicator;

    public event Action<float> OnFillingHoldKey;

    private void Start()
    {
        GetRequiredComponents();
        keyPressUI.SetActive(false);
    }

    private void GetRequiredComponents()
    {
        keyPressLabel = keyPressUI.transform.GetChildWithName("Label").GetComponent<Text>();
        keyHoldFill = keyPressUI.transform.GetChildWithName("Fill").GetComponent<Image>();
        ghostsIndicator = gameplayUI.transform.GetChildWithName("GhostsIndicator").gameObject;
        specialGhostsIndicator = ghostsIndicator.transform.GetChildWithName("SpecialGhostsIndicator").gameObject;
    }

    private void Update()
    {
        KeyPressHandler();

        if (Input.GetKeyDown(KeyCode.V))
            ShowGameplayUI();
    }

    public void ShowGameplayUI()
    {
        gameplayUI.SetActive(true);

        if (gameplayUICoroutine != null)
            StopCoroutine(gameplayUICoroutine);

        gameplayUICoroutine = this.WaitAndDo(() =>
        {
            HideGameplayUI();
            gameplayUICoroutine = null;
        }, 5f);
    }

    public void HideGameplayUI() => gameplayUI.SetActive(false);

    public void EnableGhostsIndicator() => ghostsIndicator.SetActive(true);

    public void EnableSpecialGhostsIndicator()
    {
        print(specialGhostsIndicator.name);
        specialGhostsIndicator.SetActive(true);
    }

    private void KeyPressHandler()
    {
        if (keyPressUI.activeInHierarchy)
        {
            if (keyPressHoldMode)
            {
                if (Input.GetKey(KeyCode.E))
                {
                    keyHoldTimer += Time.deltaTime * keyHoldFillSpeed;

                    if (keyHoldTimer >= 1f)
                    {
                        KeyHoldingFinished?.Invoke();
                        keyHoldFill.fillAmount = 0f;
                        HidePressKeyUI(keyPressOwner);
                    }
                }
                else if (keyHoldTimer > 0f)
                {
                    keyHoldTimer -= Time.deltaTime * keyHoldFillSpeed * 1.5f;
                }

                keyHoldFill.fillAmount = keyHoldTimer;

                if (keyHoldTimer > 0f)
                    OnFillingHoldKey?.Invoke(keyHoldTimer);
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    KeyHoldingFinished?.Invoke();
                    HidePressKeyUI(keyPressOwner);
                }
            }
        }
        else
        {
            keyHoldTimer = keyHoldFill.fillAmount = 0f;
        }
    }

    public void ShowPressKeyUI(string label, MonoBehaviour owner, bool holdMode, Action onFinishedHolding) => ShowPressKeyUI(label, owner, holdMode, 2.25f, onFinishedHolding);

    public void ShowPressKeyUI(string label, MonoBehaviour owner, bool holdMode, float speed, Action onFinishedHolding)
    {
        keyHoldFillSpeed = speed;
        keyPressLabel.text = label + (holdMode ? " (Hold)" : "");
        keyPressOwner = owner;
        KeyHoldingFinished = onFinishedHolding;
        keyPressHoldMode = holdMode;
        keyPressUI.SetActive(true);
    }

    public bool IsOwnerOfPressKey(MonoBehaviour toCheck) => keyPressOwner == toCheck; 

    public void HidePressKeyUI(MonoBehaviour owner)
    {
        if (keyPressOwner == owner && keyPressUI.activeInHierarchy)
        {
            keyPressUI.SetActive(false);
            KeyHoldingFinished = null;
            keyPressOwner = null;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[GameManagerMember]
public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private float regenerateWaitTime = 5f;
    [SerializeField] private float regenerateDelay = 0.1f;
    [Space]
    [SerializeField] private float regenerateDashWaitTime = 5f;
    [Space]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image dashBar;
    [SerializeField] private GameObject deathScren; 

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsAlive => CurrentHealth > 0;
    public bool IsDashAvailable { get; private set; } = true;

    private float dashTimer = 0f;
    private IEnumerator regenCorutine;
    private bool canRegenerate = false;
    private float regenTimer = 0f;

    private void Update()
    {
        if (!IsDashAvailable)
            dashTimer += Time.deltaTime;

        dashBar.transform.parent.gameObject.SetActive(GameManager.ResourcesManager.HasUnlockedSkill(Skills.Dash));

        dashBar.fillAmount = Mathf.Lerp(dashBar.fillAmount, IsDashAvailable ? 1f : dashTimer / regenerateDashWaitTime, Time.deltaTime * 7.5f);

        if (canRegenerate && currentHealth < maxHealth)
        {
            regenTimer += Time.deltaTime;

            if (regenTimer >= regenerateDelay)
            {
                regenTimer = 0f;
                currentHealth++;
                UpdateUI();
            }
        }
    }

    public void DealDamage(int damage)
    {
        if (!IsAlive)
            return;

        if (currentHealth - damage <= 0)
            currentHealth = 0;
        else
            currentHealth -= damage;

        canRegenerate = false;

        if (regenCorutine != null)
            StopCoroutine(regenCorutine);

        regenCorutine = this.WaitAndDo(() => canRegenerate = true, regenerateWaitTime);

        UpdateUI();

        if (!IsAlive)
            ImmediatelyDie();
    }

    public void UseDash()
    {
        if (!IsDashAvailable)
            return;

        dashTimer = 0f;
        dashBar.fillAmount = 0f;
        IsDashAvailable = false;
        this.WaitAndDo(() => IsDashAvailable = true, regenerateDashWaitTime);
    }

    private void UpdateUI() => healthBar.fillAmount = (float)currentHealth / maxHealth;

    public void ImmediatelyDie()
    {
        this.WaitAndDo(() =>
        {
            deathScren.SetActive(true);
            CursorController.ShowCursor();
        }, 3f);
        GameManager.PlayerController.Death();
        GameManager.CameraContoller.enabled = false;
    }
}

using UnityEngine;
using Action = System.Action;

public class Ghost : BaseAI, IHittable
{
    public string state = "";

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;
    [Space]
    [SerializeField] private int minDamage = 10;
    [SerializeField] private int maxDamage = 10;

    [Header("Patrol")]
    [SerializeField] private StraightPath path;
    [SerializeField] private float patrolSpeed = 4f;

    [Header("Chase")]
    [SerializeField] private float distanceToChase = 5f;
    [SerializeField] private float distanceToAttack = 1f;
    [SerializeField] private float chaseSpeed = 5f;

    [Header("Attack")]
    [SerializeField] private float minAttackDelay = 0.5f;
    [SerializeField] private float maxAttackDelay = 1.2f;
    [SerializeField] private float teleportProbability = 1f;
    [SerializeField] private float attackSpeed = 7f;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int RandomDamage => Random.Range(minDamage, maxDamage);
    public float DistanceToAttack => distanceToAttack;
    public float AttackSpeed => attackSpeed;
    public bool CanHit { get; private set; } = true;
    public bool IsHitting { get; private set; } = false;
    private bool isMovingRight = false;
    private bool callTeleportBehindPlayer = false;
    private float prevXPos;
    private float startYPos;
    public StraightPath Path { get => path; set => path = value; }

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Color targetColor;

    public event Action OnDeath;

    public bool HasTarget => CurrentTarget != null;
    public Transform CurrentTarget { get; private set; }

    private float DistanceToTarget => HasTarget ? Vector2.Distance(transform.position, CurrentTarget.position) : float.PositiveInfinity;
    private float TargetHeightDiff => Mathf.Abs(Mathf.Abs(GameManager.PlayerReference.transform.position.y) - Mathf.Abs(transform.position.y));

    protected override void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startYPos = transform.position.y;
        prevXPos = transform.position.x;
        targetColor = Color.black;
        GameManager.PlayerReference.GetComponent<FightController>().OnAttack += OnPlayerAttack;

        if (GameManager.ResourcesManager.HasUnlockedSkill(Skills.GhostsLowerHealth))
            currentHealth = Mathf.RoundToInt(currentHealth * 0.8f);
    }

    protected override void Update()
    {
        base.Update();
        ScanForPlayer();
        transform.position = new Vector3(transform.position.x, startYPos, transform.position.z);
        state = StateMachine.CurrentState.ToString();
        isMovingRight = prevXPos < transform.position.x;
        prevXPos = transform.position.x;
        spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * 4.5f);
        YSwaper();
    }

    private void YSwaper()
    {
        float yRot;

        if (HasTarget)
            yRot = CurrentTarget.position.x > transform.position.x ? 0f : 180f;
        else
            yRot = isMovingRight ? 0f : 180f;

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRot, transform.eulerAngles.z);
    }

    private void ScanForPlayer()
    {
        if (Vector2.Distance(GameManager.PlayerReference.transform.position, transform.position) <= distanceToChase && TargetHeightDiff <= 3f)
        {
            CurrentTarget = GameManager.PlayerReference.transform;
        }
        else
        {
            CurrentTarget = null;
        }
    }

    protected override void InitializeStateMachine()
    {
        base.InitializeStateMachine();

        GhostStates.Patrol patrol = new GhostStates.Patrol(transform, patrolSpeed, path);
        GhostStates.ChasePlayer chasePlayer = new GhostStates.ChasePlayer(this, chaseSpeed);
        GhostStates.WaitForHit waitForHit = new GhostStates.WaitForHit(this, minAttackDelay, maxAttackDelay);
        GhostStates.Hit hit = new GhostStates.Hit(this);
        GhostStates.TeleportBehindPlayer teleportBehindPlayer = new GhostStates.TeleportBehindPlayer(this);

        StateMachine.AddTransition(patrol, chasePlayer, () => CurrentTarget != null);
        StateMachine.AddTransition(chasePlayer, patrol, () => CurrentTarget == null);
        StateMachine.AddTransition(chasePlayer, waitForHit, () => DistanceToTarget <= distanceToAttack);
        StateMachine.AddTransition(waitForHit, hit, () => IsHitting && TargetHeightDiff < 0.2f);
        StateMachine.AddTransition(waitForHit, chasePlayer, () => DistanceToTarget > distanceToAttack);
        StateMachine.AddTransition(waitForHit, teleportBehindPlayer, () =>
        {
            if (callTeleportBehindPlayer)
            {
                callTeleportBehindPlayer = false;
                return true;
            }
            return false;
        });
        StateMachine.AddTransition(hit, waitForHit, () => !IsHitting);
        StateMachine.AddTransition(teleportBehindPlayer, waitForHit, () => !callTeleportBehindPlayer);

        StateMachine.SwitchState(patrol);
    }

    public void CallHit()
    {
        if (!IsHitting && CanHit && StateMachine.IsState<GhostStates.WaitForHit>() && CurrentTarget != null)
        {
            IsHitting = true;
            anim.SetTrigger("attack");
        }
    }

    public void ResetHitState() => IsHitting = false;

    public void GiveDamage(int damage)
    {
        if (callTeleportBehindPlayer)
            return;

        if (currentHealth - damage <= 0)
        {
            ImmediatelyDie();
        }
        else
        {
            currentHealth -= damage;
            float multi = 1f - ((float)currentHealth / maxHealth);
            targetColor = new Color(multi, multi, multi, 1f);
        }
    }

    public void ImmediatelyDie()
    {
        OnDeath?.Invoke();
        currentHealth = 0;
        GameManager.PlayerReference.GetComponent<FightController>().OnAttack -= OnPlayerAttack;
        Destroy(gameObject);
    }

    private void OnPlayerAttack()
    {
        if (StateMachine.IsState<GhostStates.WaitForHit>() && Random.Range(0f, 1f) <= teleportProbability)
        {
            callTeleportBehindPlayer = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"<color=red>Hit {Random.Range(1000, 9999)}</color>");
            GameManager.PlayerStats.DealDamage(RandomDamage);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + (transform.right * distanceToChase), transform.position - (transform.right * distanceToChase));
    }
}

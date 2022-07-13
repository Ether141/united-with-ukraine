using System.Linq;
using UnityEngine;
using Action = System.Action;

public class FightController : MonoBehaviour
{
    [SerializeField] private int minDamage = 10;
    [SerializeField] private int maxDamage = 15;
    public bool canHit = true;

    [Header("Raycast")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private Vector2 overlapBoxSize;
    [SerializeField] private Vector2 overlapBoxOffset;

    private Vector2 OverlapBoxCenter => new Vector2(transform.position.x, transform.position.y) + (overlapBoxOffset.x * (Vector2)transform.right) + (overlapBoxOffset.y * (Vector2)transform.up);

    public int RandomDamage => Mathf.RoundToInt(Random.Range(minDamage, maxDamage) * (GameManager.ResourcesManager.HasUnlockedSkill(Skills.HigherDamage) ? 1.25f : 1f));
    public bool IsHitting { get; private set; } = false;
    public bool HasAnyTarget => CurrentTargets.Length > 0;
    public Transform[] CurrentTargets { get; private set; }
    public Transform ClosestTarget
    {
        get
        {
            if (!HasAnyTarget)
                return null;

            Transform closest = CurrentTargets[0];
            float minDis = Vector2.Distance(GameManager.PlayerReference.transform.position, closest.position);

            if (CurrentTargets.Length == 0)
                return closest;

            foreach (Transform t in CurrentTargets)
            {
                float currentDis = Vector2.Distance(GameManager.PlayerReference.transform.position, t.position);
                if (currentDis < minDis)
                {
                    closest = t;
                    minDis = currentDis;
                }
            }

            return closest;
        }
    }
    private Animator stickAnim;

    public event Action OnAttack;

    private void Start() => stickAnim = transform.GetChild(0).GetComponent<Animator>();

    private void Update()
    {
        HitController();
        RaycastScanner();
    }

    private void HitController()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.LeftAlt)) && canHit && !IsHitting && GameManager.PlayerController.canMove)
        {
            stickAnim.SetTrigger("hit");
            IsHitting = true;
            this.WaitAndDo(() => HitTarget(), 0.15f);
            this.WaitAndDo(() => IsHitting = false, 0.45f);
        }
    }

    public void HitTarget()
    {
        if (HasAnyTarget)
        {
            int damage = RandomDamage;
            IHittable hittable = ClosestTarget.GetComponent<IHittable>();
            hittable.GiveDamage(damage);
            OnAttack?.Invoke();
            print($"dealt damage {damage} | to: {ClosestTarget.name} | current health: {hittable.CurrentHealth}"); 
        }
    }

    private void RaycastScanner() => CurrentTargets = Physics2D.OverlapBoxAll(OverlapBoxCenter, overlapBoxSize, 0f, targetLayer).Select(c => c.transform).ToArray();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireCube(OverlapBoxCenter, new Vector3(overlapBoxSize.x, overlapBoxSize.y, 0f));
    }
}

using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] protected string label = "Inspect";
    [SerializeField] protected bool needToHoldKey = false;

    public bool IsBeingInspected { get; protected set; } = false;
    public bool CanBeInspected { get; protected set; } = true;

    protected virtual bool AdditionalInteractConditions => true;
    protected virtual float HoldKeySpeed => 2.25f;

    protected virtual void Update()
    {
        if (GameManager.PlayerInteractableScanner.IsLookingAt(gameObject) && !IsBeingInspected && CanBeInspected && AdditionalInteractConditions)
        {
            GameManager.GameplayUIManager.ShowPressKeyUI(label, this, needToHoldKey, HoldKeySpeed, Inspect);
        }
        else if (!GameManager.PlayerInteractableScanner.IsLookingAt(gameObject))
        {
            GameManager.GameplayUIManager.HidePressKeyUI(this);
        }
    }

    public virtual void Inspect() => IsBeingInspected = true;
}

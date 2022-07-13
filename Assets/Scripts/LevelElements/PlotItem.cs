using UnityEngine;

public class PlotItem : Interactable
{
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemIcon;

    public string ItemName => itemName;

    private void Start() => GameManager.LevelManager.OnTransformToNight += RemoveItem;

    private void RemoveItem() => Destroy(gameObject);

    public override void Inspect()
    {
        base.Inspect();
        GameManager.PlotItems.AddItem(itemName, itemIcon);
        GameManager.GameplayUIManager.ShowGameplayUI();
        Destroy(gameObject);
    }

    public void Activate() => gameObject.SetActive(true);

    private void OnDestroy() => GameManager.LevelManager.OnTransformToNight -= RemoveItem;

    private void OnValidate()
    {
        label = itemName;
        needToHoldKey = true;
    }
}

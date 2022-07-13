using UnityEngine;

public class PickableResource : Interactable
{
    [SerializeField] private int minResourceProgress = 10;
    [SerializeField] private int maxResourceProgress = 20;

    protected override bool AdditionalInteractConditions => !GameManager.LevelManager.IsNight;

    public override void Inspect()
    {
        GameManager.ResourcesManager.AddProgress(Random.Range(minResourceProgress, maxResourceProgress));
        print($"picked {name}");
        Destroy(gameObject);
    }
}

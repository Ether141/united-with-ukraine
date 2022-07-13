using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Text = TMPro.TextMeshProUGUI;

public class GhostsSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private GameObject specialGhostPrefab;
    [Space]
    [SerializeField] private float minDelay = 10f;
    [SerializeField] private float maxDelay = 15f;
    [Space]
    [SerializeField] private StraightPath[] pathsToSpawn;

    [Header("UI")]
    [SerializeField] private Text ghostsCountUI;
    [SerializeField] private Text specialGhostsCountUI;

    private int targetGhostsCount = 0;
    private int targetSpecialGhostsCount = 0;
    private StraightPath[] AvailablePaths => pathsToSpawn.Where(path => !path.hasAnyAssociatedGhost).ToArray();
    private List<Ghost> SpawnedGhosts = new List<Ghost>();
    private List<Ghost> SpawnedSpecialGhosts = new List<Ghost>();
    public int SpawnedGhostsCount => SpawnedGhosts.Count;
    public int TotalSavedGhostsCount { get; private set; } = 0;
    public int TotalSpawnedGhostsCount { get; private set; } = 0;
    public int TotalSavedSpecialGhostsCount { get; private set; } = 0;
    public bool AllGhostsSaved => targetGhostsCount == TotalSavedGhostsCount && targetGhostsCount > 0;

    public void StartSpawningProcess(int targetGhostsCount, SpecialGhost[] specialGhosts)
    {
        this.targetGhostsCount = targetGhostsCount;
        targetSpecialGhostsCount = specialGhosts.Length;
        GameManager.GameplayUIManager.EnableGhostsIndicator();
        UpdateUI();
        SpawnGhost();
        SpawnSpecialGhosts(specialGhosts);
    }

    private void SpawnGhost()
    {
        if (AllGhostsSaved || TotalSpawnedGhostsCount == targetGhostsCount)
            return;

        if (AvailablePaths.Length == 0)
        {
            this.WaitAndDo(SpawnGhost, Random.Range(minDelay, maxDelay));
            return;
        }

        TotalSpawnedGhostsCount++;
        StraightPath path = AvailablePaths[Random.Range(0, AvailablePaths.Length)];
        Ghost newGhost = Instantiate(ghostPrefab, path.StartPosition, Quaternion.identity).GetComponent<Ghost>();
        newGhost.Path = path;
        path.hasAnyAssociatedGhost = true;
        SpawnedGhosts.Add(newGhost);
        newGhost.OnDeath += () => RemoveGhost(newGhost, path);
        this.WaitAndDo(SpawnGhost, Random.Range(minDelay, maxDelay));
    }

    private void SpawnSpecialGhosts(SpecialGhost[] specialGhosts)
    {
        foreach (var ghost in specialGhosts)
        {
            if (!GameManager.PlotItems.HasCollectedItem(ghost.plotItem.ItemName))
                continue;

            Ghost newGhost = Instantiate(specialGhostPrefab, ghost.path.StartPosition, Quaternion.identity).GetComponent<Ghost>();
            newGhost.Path = ghost.path;
            ghost.path.hasAnyAssociatedGhost = true;
            SpawnedSpecialGhosts.Add(newGhost);
            newGhost.OnDeath += () => RemoveSpecialGhost(newGhost, ghost.path);
        }
    }

    private void EndSpawningProcess()
    {
        print("all ghosts slayered");
    }

    private void UpdateUI()
    {
        GameManager.GameplayUIManager.ShowGameplayUI();
        ghostsCountUI.text = $"{TotalSavedGhostsCount} / {targetGhostsCount}";
        specialGhostsCountUI.text = $"{TotalSavedSpecialGhostsCount} / {targetSpecialGhostsCount}";
    }

    private void RemoveSpecialGhost(Ghost ghost, StraightPath path)
    {
        if (!SpawnedSpecialGhosts.Contains(ghost))
            return;

        path.hasAnyAssociatedGhost = false;
        SpawnedSpecialGhosts.Remove(ghost);
        TotalSavedSpecialGhostsCount++;
        UpdateUI();
    }

    private void RemoveGhost(Ghost ghost, StraightPath path)
    {
        if (!SpawnedGhosts.Contains(ghost))
            return;

        path.hasAnyAssociatedGhost = false;
        SpawnedGhosts.Remove(ghost);
        TotalSavedGhostsCount++;
        UpdateUI();

        if (AllGhostsSaved)
            EndSpawningProcess();
    }
}

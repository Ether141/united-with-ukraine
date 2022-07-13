using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[GameManagerMember]
public class LevelManager : MonoBehaviour
{
    [Header("Ghosts")]
    [SerializeField] private int totalGhostsCount = 10;
    [SerializeField] private GhostsSpawner ghostsSpawner;
    [SerializeField] private SpecialGhost[] specialGhosts;

    [Header("Lighting")]
    [SerializeField] private Light2D globalLighting;
    [SerializeField] private float lightTransformSpeed = 0.5f;
    [SerializeField] private float nightLightIntensity = 0.125f;

    private float targetLightIntensity;
    public GhostsSpawner GhostsSpawner => ghostsSpawner;

    public event Action OnTransformToNight;

    public bool IsNight { get; private set; } = false;

    private void Start()
    {
        targetLightIntensity = globalLighting.intensity;
    }

    private void Update()
    {
        globalLighting.intensity = Mathf.Lerp(globalLighting.intensity, targetLightIntensity, Time.deltaTime * lightTransformSpeed);
    }

    public void ActivateSpecialGhost(PlotItem plotItem)
    {
        SpecialGhost specialGhost = specialGhosts.FirstOrDefault(g => g.plotItem.ItemName == plotItem.ItemName);

        if (specialGhost == null)
            return;

        GameManager.GameplayUIManager.EnableSpecialGhostsIndicator();
        specialGhost.isActive = true;
        plotItem.Activate();
    }

    public void TransformToNight()
    {
        if (IsNight)
            return;

        OnTransformToNight?.Invoke();
        IsNight = true;
        targetLightIntensity = nightLightIntensity;
        SpecialGhost[] sg = specialGhosts.Where(g => g.isActive).ToArray();
        this.WaitAndDo(() => ghostsSpawner.StartSpawningProcess(totalGhostsCount, sg), 7f);
    }
}

[Serializable]
public class SpecialGhost
{
    public bool isActive = false;
    public PlotItem plotItem;
    public StraightPath path;
}

using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Campfire : Interactable
{
    [SerializeField] private float fovChange = 4f;
    [SerializeField] private AudioClip nightClip;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private GameObject startLabel;

    private Light2D light2D;
    private ParticleSystem particle;
    private AudioSource soundPlayer;

    private float targetLightIntensity = 0f;
    private float targetAudioVolume = 0f;
    private float perlinSeed;
    private bool reachedTargetIntensity = false;
    private float prevProgress = 0f;
    private float targetMusicVolume = 1f;

    private const float Speed = 0.8f;
    private const float MinIntensity = 0.3f;
    private const float MaxIntensity = 0.7f;

    public bool IsIgnited { get; private set; } = false;
    protected override float HoldKeySpeed => 0.275f;

    private AudioSource loadingSound;

    private void Start()
    {
        perlinSeed = Random.Range(0f, 65535f);
        light2D = transform.GetChildWithName("Light").GetComponent<Light2D>();
        particle = transform.GetChildWithName("Fire").GetComponent<ParticleSystem>();
        soundPlayer = GetComponent<AudioSource>();
        GameManager.GameplayUIManager.OnFillingHoldKey += GameplayUIManager_OnFillingHoldKey;
        loadingSound = transform.GetChildWithName("LoadingSound").GetComponent<AudioSource>();
    }

    private void GameplayUIManager_OnFillingHoldKey(float progress)
    {
        if (progress <= 0.05f && loadingSound.isPlaying)
            loadingSound.Stop();

        if (GameManager.GameplayUIManager.IsOwnerOfPressKey(this))
        {
            if (!loadingSound.isPlaying)
                loadingSound.Play();
             
            if (loadingSound.isPlaying)
            {
                if (prevProgress > progress && loadingSound.pitch > 0)
                    loadingSound.pitch *= -1.5f;
                else if (prevProgress < progress && loadingSound.pitch < 0)
                    loadingSound.pitch /= -1.5f;
            }

            GameManager.PlayerController.canMove = loadingSound.pitch < 0 || progress > 0.95f;
            loadingSound.volume = loadingSound.pitch < 0 ? 0f : 1f;
            targetMusicVolume = loadingSound.pitch < 0 ? 1f : 0f;

            GameManager.CameraContoller.ForceFocus();
            GameManager.CameraContoller.ChangeFOV(GameManager.CameraContoller.StartFov - (fovChange * progress), 10f);
            prevProgress = progress;
        }
    }

    protected override void Update()
    {
        base.Update();
        musicSource.volume = Mathf.Lerp(musicSource.volume, targetMusicVolume, Time.deltaTime * (musicSource.clip == nightClip ? 0.5f : 1.2f));

        if (IsIgnited)
        {
            soundPlayer.volume = Mathf.Lerp(soundPlayer.volume, targetAudioVolume, Time.deltaTime * Speed);

            if (!reachedTargetIntensity)
            {
                light2D.intensity = Mathf.Lerp(light2D.intensity, targetLightIntensity, Time.deltaTime * Speed);
                if (light2D.intensity >= targetLightIntensity * 0.9f)
                    reachedTargetIntensity = true;
            }

            if (reachedTargetIntensity)
                light2D.intensity = Mathf.Lerp(MinIntensity, MaxIntensity, Mathf.PerlinNoise(perlinSeed, Time.time));
        }
    }

    public override void Inspect()
    {
        base.Inspect();
        Ignite();
        GameManager.CameraContoller.ResetFOV(5.5f);
    }

    private void Ignite()
    {
        IsIgnited = true;
        particle.Play(true);
        soundPlayer.Play();
        targetLightIntensity = 0.5f;
        musicSource.clip = nightClip;
        targetAudioVolume = 1f;
        GameManager.LevelManager.TransformToNight();

        this.WaitAndDo(() =>
        {
            startLabel.SetActive(true);

            this.WaitAndDo(() =>
            {
                targetMusicVolume = 1f;
                musicSource.Play();
            }, 4f);
        }, 3f);
    }

    private void Reset()
    {
        label = "Campfire - ignite";
        needToHoldKey = true;
    }
}

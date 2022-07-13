using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource audioSource;
    public float targetVolume = 1f;

    private int screenIndex = 0;

    private void Start() => ActivateScreen(0);

    private void Update() => audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * 4f);

    public void ActivateScreen(int index)
    {
        screenIndex = index;
        anim.SetInteger("screen", screenIndex);
    }

    public void OnHoverButton(Button sender)
    {
        var particle = sender.transform.GetChildWithName("Particles").GetComponent<ParticleSystem>().main;
        var particleB = sender.transform.GetChildWithName("Particles").GetChild(0).GetComponent<ParticleSystem>().main;
        particleB.simulationSpeed = particle.simulationSpeed *= 2f;
    }

    public void OnLeaveButton(Button sender)
    {
        var particle = sender.transform.GetChildWithName("Particles").GetComponent<ParticleSystem>().main;
        var particleB = sender.transform.GetChildWithName("Particles").GetChild(0).GetComponent<ParticleSystem>().main;
        particleB.simulationSpeed = particle.simulationSpeed /= 2f;
    }

    public void CloseGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

using UnityEngine;

[GameManagerMember]
public class SoundsProvider : MonoBehaviour
{
    private Transform soundsObjectsRoot;
    
    // Sounds list
    public const string ExampleSound = "ExampleSound";

    private void Awake() => soundsObjectsRoot = GameObject.Find("-- Sounds --").transform;

    public AudioSource GetSound(string soundName)
    {
        foreach (Transform child in soundsObjectsRoot)
            if (child.name == soundName)
                return child.GetComponent<AudioSource>();
        Debug.LogError($"Unable to found sound with given name: {soundName}");
        return null;
    }

    public void PlaySound(string soundName) => GetSound(soundName)?.Play();

    public void CloneAndPlaySound(string soundName)
    {
        AudioSource sound = GetSound(soundName);

        if (sound != null)
        {
            AudioSource newSound = Instantiate(sound.gameObject).GetComponent<AudioSource>();
            Destroy(newSound.gameObject, newSound.clip.length + 1f);
            newSound.playOnAwake = false;
            newSound.Play();
        }
    }
}

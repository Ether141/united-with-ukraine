using UnityEngine;
using Text = TMPro.TextMeshProUGUI;

[GameManagerMember]
public class DialogueDisplayer : MonoBehaviour
{
    [SerializeField] private Text dialogueTextUI;
    [SerializeField] private GameObject keyUI;

    public void DisplayText(string text) => dialogueTextUI.text = text;

    public void ClearUI()
    {
        dialogueTextUI.text = string.Empty;
        EnableKeyUI(false);
    }

    public void EnableKeyUI(bool enable) => keyUI.SetActive(enable);
}

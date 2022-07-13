using UnityEngine;

public class SpeakerCharacter : Interactable
{
    [SerializeField] private float playerDistance = 2f;
    [SerializeField] private string[] texts;
    [SerializeField] private PlotItem plotItem;

    protected override bool AdditionalInteractConditions => !GameManager.LevelManager.IsNight;

    private int currentTextIndex = 0;
    private bool canNextText = false;

    public override void Inspect()
    {
        base.Inspect();
        StartDialogue();
    }

    private void StartDialogue()
    {
        CanBeInspected = false;
        GameManager.PlayerController.canMove = false;
        GameManager.CameraContoller.ChangeFOV(7.5f, 2f);
        GameManager.CameraContoller.ForceFocus();

        bool isPlayerTowards = (GameManager.PlayerReference.transform.position.x > transform.position.x && transform.eulerAngles.y == 0f) ||
                               (GameManager.PlayerReference.transform.position.x < transform.position.x && transform.eulerAngles.y == 180f);

        if (!isPlayerTowards)
            transform.localEulerAngles = new Vector3(0f, transform.eulerAngles.y == 0f ? 180f : 0f, 0f);

        Vector2 playerPos = (Vector2)transform.position + (Vector2)(transform.right * playerDistance);
        GameManager.PlayerController.MoveTo(playerPos, 2f);

        canNextText = false;
        this.WaitAndDo(() =>
        {
            NextText();
            canNextText = true;
        }, 2f);
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Space) && IsBeingInspected && currentTextIndex > 0 && canNextText)
        {
            canNextText = false;
            NextText();
        }
    }

    private void NextText()
    {
        if (currentTextIndex >= texts.Length)
        {
            EndDialogue();
        }
        else
        {
            GameManager.DialogueDisplayer.EnableKeyUI(true);
            GameManager.DialogueDisplayer.DisplayText(texts[currentTextIndex++]);
            canNextText = true;
        }
    }

    private void EndDialogue()
    {
        GameManager.DialogueDisplayer.ClearUI();
        GameManager.CameraContoller.ResetFOV(3f);
        if (plotItem != null)
            GameManager.LevelManager.ActivateSpecialGhost(plotItem);
        this.WaitForEndOfFrame(() => GameManager.PlayerController.canMove = true);
    }

    private void OnValidate()
    {
        label = $"Talk";
        needToHoldKey = true;
    }
}

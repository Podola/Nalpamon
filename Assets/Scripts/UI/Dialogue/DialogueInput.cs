using System.Threading;
using TMPro;
using UnityEngine.InputSystem;
using Yarn.Markup;
using Yarn.Unity;

public class DialogueInput : Singleton<DialogueInput>, IActionMarkupHandler
{
    private DialogueRunner runner;

    private InputAction skipKeyAction;

    private uint blockCount = 0;

    private bool lineDisplay;

    public bool Block
    {
        get => blockCount > 0;
        set
        {
            if (value) blockCount++;
            else blockCount--;
        }
    }

    private void Start()
    {
        runner = FindFirstObjectByType<DialogueRunner>();

        CustomLinePresenter lineView = FindFirstObjectByType<CustomLinePresenter>();

        if (lineView != null) lineView.temporalProcessors.Add(this);

        skipKeyAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/space");
        skipKeyAction.performed += ctx => OnInput();
        skipKeyAction.Enable();
    }

    private void OnDestroy()
    {
        skipKeyAction.Disable();
    }

    public void OnInput()
    {
        if (Block) return;
        if (!runner.IsDialogueRunning) return;

        if (lineDisplay) runner.RequestHurryUpLine();
        else runner.RequestNextLine();
    }

    #region Yarn
    public void OnPrepareForLine(MarkupParseResult line, TMP_Text text)
    {
        lineDisplay = true;
    }

    public void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
    {
        lineDisplay = true;
    }

    public YarnTask OnCharacterWillAppear(int currentCharacterIndex, MarkupParseResult line, CancellationToken cancellationToken)
    {
        return YarnTask.CompletedTask;
    }

    public void OnLineDisplayComplete()
    {
        lineDisplay = false;
    }

    public void OnLineWillDismiss()
    {
        lineDisplay = false;
    }
    #endregion
}

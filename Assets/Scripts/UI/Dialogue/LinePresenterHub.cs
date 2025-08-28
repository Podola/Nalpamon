/*
Yarn Spinner is licensed to you under the terms found in the file LICENSE.md.
*/

using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Yarn.Unity;

public class LinePresenterHub : DialoguePresenterBase
{
    [SerializeField] private DialoguePresenterBase[] linePresenters;

    private int index;

    private readonly List<IPresenterEvent> conditions = new();

    private void Start()
    {
        DialogueRunner runner = FindFirstObjectByType<DialogueRunner>();

        runner.AddPresenter(this);
    }

    public override YarnTask OnDialogueCompleteAsync()
    {
        if (linePresenters.Length == 0) return YarnTask.CompletedTask;

        return linePresenters[index].OnDialogueCompleteAsync();
    }

    public override YarnTask OnDialogueStartedAsync()
    {
        if (linePresenters.Length == 0) return YarnTask.CompletedTask;

        return linePresenters[index].OnDialogueStartedAsync();
    }

    public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        if (linePresenters.Length != 0)
        {
            string tag = line.GetLineTag("LineIndex:");
    
            if (tag != "") index = int.Parse(tag);
    
            for (int i = 0; i < conditions.Count; i++)
            {
                conditions[i].LineEvent(line);
    
                await conditions[i].LineWait(token);
            }
    
            await linePresenters[index].RunLineAsync(line, token);
        }
    }

    public override YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        if (linePresenters.Length == 0) return YarnTask<DialogueOption>.FromResult(null);

        return linePresenters[index].RunOptionsAsync(dialogueOptions, cancellationToken);
    }

    public void AddPresenterEvent(IPresenterEvent presenterEvent)
    {
        if (!conditions.Contains(presenterEvent)) conditions.Add(presenterEvent);
    }

    public void RemovePresenterEvent(IPresenterEvent presenterEvent)
    {
        conditions.Remove(presenterEvent);
    }
}

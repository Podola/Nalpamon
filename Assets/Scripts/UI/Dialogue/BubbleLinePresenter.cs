/*
Yarn Spinner is licensed to you under the terms found in the file LICENSE.md.
*/

using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using Yarn.Markup;
using Yarn.Unity;

public sealed class BubbleLinePresenter : DialoguePresenterBase
{
    private CustomLinePresenter linePresenter;

    private readonly Dictionary<string, BubbleLineTarget> targets = new();

    private void Awake()
    {
        Pool = new ObjectPool<BubbleLineHandle>(CreateObject, OnGetObject, OnReleseObject, OnDestroyObject);

        linePresenter = FindAnyObjectByType<CustomLinePresenter>();

        DialogueRunner runner = FindFirstObjectByType<DialogueRunner>();

        runner.AddCommandHandler<string, string>("PlayAnimation", PlayAnimation);
    }

    public void AddTarget(BubbleLineTarget target)
    {
        targets.Add(target.ID, target);

        target.Handle = Pool.Get();
    }

    public void RemoveTarget(BubbleLineTarget target)
    {
        targets.Remove(target.ID);

        target.Handle.Release();
    }

    public void PlayAnimation(string target, string animationName)
    {
        if (targets.ContainsKey(target)) targets[target].PlayAnimayion(animationName);
    }

    #region Yarn
    public override YarnTask OnDialogueCompleteAsync()
    {
        return YarnTask.CompletedTask;
    }

    public override YarnTask OnDialogueStartedAsync()
    {
        return YarnTask.CompletedTask;
    }

    public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        string characterName = line.CharacterName;

        if (targets.ContainsKey(characterName))
        {
            BubbleLineTarget target = targets[characterName];

            TMP_Text lineText = target.Handle.BubbleText;
            MarkupParseResult text = line.TextWithoutCharacterName;

            target.Handle.SetActive(true);

            lineText.text = text.Text;

            if (linePresenter.useTypewriterEffect)
            {
                lineText.maxVisibleCharacters = 0;

                foreach (var processor in linePresenter.temporalProcessors) processor.OnPrepareForLine(text, lineText);
            }
            else lineText.maxVisibleCharacters = text.Text.Length;

            if (linePresenter.useTypewriterEffect)
            {
                foreach (var processor in linePresenter.temporalProcessors) processor.OnLineDisplayBegin(text, lineText);

                int milliSecondsPerLetter = 0;

                if (linePresenter.typewriterEffectSpeed > 0) milliSecondsPerLetter = (int)(1000f / linePresenter.typewriterEffectSpeed);

                for (int i = 0; i < text.Text.Length; i++)
                {
                    foreach (var processor in linePresenter.temporalProcessors)
                    {
                        await processor.OnCharacterWillAppear(i, text, token.HurryUpToken).SuppressCancellationThrow();
                    }

                    lineText.maxVisibleCharacters += 1;

                    if (milliSecondsPerLetter > 0)
                    {
                        await YarnTask.Delay(System.TimeSpan.FromMilliseconds(milliSecondsPerLetter), token.HurryUpToken).SuppressCancellationThrow();
                    }
                }

                lineText.maxVisibleCharacters = text.Text.Length;

                foreach (var processor in linePresenter.temporalProcessors) processor.OnLineDisplayComplete();
            }

            if (linePresenter.autoAdvance) await YarnTask.Delay((int)(linePresenter.autoAdvanceDelay * 1000), token.NextLineToken).SuppressCancellationThrow();
            else await YarnTask.WaitUntilCanceled(token.NextLineToken).SuppressCancellationThrow();

            foreach (var processor in linePresenter.temporalProcessors) processor.OnLineWillDismiss();

            target.Handle.SetActive(false);
        }
    }

    public override YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        return YarnTask<DialogueOption>.FromResult(null);
    }
    #endregion

    #region 풀링
    [Header("Pool Settings")]
    [SerializeField] private Transform parent;
    [SerializeField] private BubbleLineHandle prefab;

    public IObjectPool<BubbleLineHandle> Pool { get; private set; }

    private BubbleLineHandle CreateObject()
    {
        BubbleLineHandle handle = Instantiate(prefab, parent);

        handle.SetPool(Pool);

        return handle;
    }

    private void OnGetObject(BubbleLineHandle handle) => handle.gameObject.SetActive(false);

    private void OnReleseObject(BubbleLineHandle handle)
    {
        if (this == null)
        {
            OnDestroyObject(handle);

            return;
        }

        handle.transform.SetParent(transform);
        handle.gameObject.SetActive(false);
    }

    private void OnDestroyObject(BubbleLineHandle handle) => Destroy(handle.gameObject);
    #endregion
}

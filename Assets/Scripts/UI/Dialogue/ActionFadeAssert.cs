using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Components;

public class ActionFadeAssert : ActionFadeBase
{
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Animator animator;

    [Header("Texts")]
    [SerializeField] private LocalizeStringEvent title;
    [SerializeField] private LocalizeStringEvent main;
    [SerializeField] private LocalizeStringEvent sub;

    private string phaseID;

    protected override void Start()
    {
        base.Start();

        animator.Play("Enactive");
    }

    public override void OnEvent(string eventName)
    {
        if (eventName == "Close") animator.Play("Close");
        else phaseID = eventName;
    }

    protected override IEnumerator FadeIn()
    {
        InterroPhaseInfoSO infoSO = ChapterDataManager.Instance.InterroPhaseDict[phaseID];

        title.SetTable(infoSO.Table, infoSO.title);

        main.SetTable(infoSO.Table, infoSO.main);
        sub.SetTable(infoSO.Table, infoSO.sub);

        yield return fadeCanvasGroup.DOFade(1f, 0.5f).WaitForCompletion();

        animator.speed = 0;
        animator.Play("Open");
    }

    protected override IEnumerator FadeOut()
    {
        yield return fadeCanvasGroup.DOFade(0f, 0.5f).WaitForCompletion();

        animator.speed = 1;

        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Active"));
    }
}

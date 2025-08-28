using DG.Tweening;
using System.Collections;
using UnityEngine;

public class ActionFadeSimple : ActionFadeBase
{
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [SerializeField, Range(0, 1)] private float fadeAlpha = 1f;

    protected override IEnumerator FadeIn()
    {
        fadeCanvasGroup.interactable = true;
        fadeCanvasGroup.blocksRaycasts = true;

        yield return fadeCanvasGroup.DOFade(fadeAlpha, 0.5f).WaitForCompletion();
    }

    protected override IEnumerator FadeOut()
    {
        yield return fadeCanvasGroup.DOFade(0f, 0.5f).WaitForCompletion();

        fadeCanvasGroup.interactable = false;
        fadeCanvasGroup.blocksRaycasts = false;
    }
}

using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면 페이더 클래스입니다.
/// </summary>
public static class ScreenFader
{
    private static CanvasGroup _cg;
    private static Tween _tween;

    public static IEnumerator FadeOutRoutine(float duration)
    {
        var canvas = EnsureCanvas();
        canvas.blocksRaycasts = true;
        _tween?.Kill();
        _tween = canvas.DOFade(1f, duration).SetUpdate(true).SetRecyclable();

        yield return _tween.WaitForCompletion();
    }

    public static IEnumerator FadeInRoutine(float duration)
    {
        var canvas = EnsureCanvas();
        _tween?.Kill();
        _tween = canvas.DOFade(0f, duration).SetUpdate(true).SetRecyclable();

        yield return _tween.WaitForCompletion();
        canvas.blocksRaycasts = false;
    }

    #region Internal Methods
    private static CanvasGroup EnsureCanvas()
    {
        if (_cg != null)
        {
            return _cg;
        }

        var obj = new GameObject("ScreenFader");
        Object.DontDestroyOnLoad(obj);

        var canvas = obj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        var scaler = obj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var img = new GameObject("FadeImage").AddComponent<Image>();
        img.transform.SetParent(canvas.transform, false);
        img.rectTransform.anchorMin = Vector2.zero;
        img.rectTransform.anchorMax = Vector2.one;
        img.color = Color.black;

        _cg = img.gameObject.AddComponent<CanvasGroup>();
        _cg.alpha = 0f;
        _cg.blocksRaycasts = false;

        return _cg;
    }
    #endregion
}
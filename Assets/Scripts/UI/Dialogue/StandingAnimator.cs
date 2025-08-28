using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandingAnimator
{
    public bool IsComplete => playCount == 0 && waitAnimations.Count == 0;

    private int playCount;
    private bool stopAnim;

    private readonly StandingDisplay standingDisplay;

    private readonly List<IEnumerator> waitAnimations = new();

    public StandingAnimator(StandingDisplay standingDisplay) => this.standingDisplay = standingDisplay;

    public void Play() => standingDisplay.StartCoroutine(WaitAndPlay());

    public IEnumerator PlayForce()
    {
        yield return WaitAndPlay();

        yield return new WaitUntil(() => playCount == 0);
    }

    private void Add(IEnumerator anim, float duration)
    {
        if (duration == 0) standingDisplay.StartCoroutine(anim);
        else waitAnimations.Add(anim);
    }

    private IEnumerator WaitAndPlay()
    {
        stopAnim = true;

        yield return new WaitUntil(() => playCount == 0);

        stopAnim = false;

        foreach (IEnumerator animation in waitAnimations) standingDisplay.StartCoroutine(animation);

        yield return new WaitUntil(() => playCount != 0);

        waitAnimations.Clear();
    }

    public void SetPos(CharStandingHandle handle, Vector2 pos, float duration = 0) => Add(MovePosAnim(handle, pos, duration), duration);

    public void SetAngle(CharStandingHandle handle, bool right, float duration = 0) => Add(RotateAnim(handle, right, duration), duration);

    public void SetSize(CharStandingHandle handle, Vector2 size, float duration) => Add(FocusAnim(handle, size, duration), duration);

    public void OutAndInOther(CharStandingHandle handle, StandingDisplayInfo spriteInfo, Vector2 outPos, Vector2 inPos, Vector2 size, float duration) => Add(OutAndInAnim(handle, spriteInfo, true, outPos, inPos, size, duration), duration);

    public void OutAndInSame(CharStandingHandle handle, StandingDisplayInfo spriteInfo, Vector2 outPos, Vector2 inPos, Vector2 size, float duration) => Add(OutAndInAnim(handle, spriteInfo, false, outPos, inPos, size, duration), duration);

    public void OutAndHide(CharStandingHandle handle, StandingDisplayInfo spriteInfo, Vector2 outPos, float duration) => Add(OutAndHideAnim(handle, spriteInfo, outPos, duration), duration);

    private IEnumerator MovePosAnim(CharStandingHandle handle, Vector2 pos, float duration)
    {
        playCount++;

        if (duration > 0)
        {
            Vector2 startPos = handle.Rect.anchoredPosition;
            float elapsedTime = 0f;

            while (!stopAnim && elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                handle.Rect.anchoredPosition = Vector2.Lerp(startPos, pos, Mathf.Clamp01(elapsedTime / duration));

                yield return null;
            }
        }

        handle.Rect.anchoredPosition = pos;

        playCount--;
    }

    private IEnumerator RotateAnim(CharStandingHandle handle, bool right, float duration)
    {
        playCount++;

        Vector3 angle = right ? new(0, -90, 0) : new(0, 90, 0);

        if (duration > 0)
        {
            Vector3 startAngle = handle.Rect.localEulerAngles;
            Vector3 endAngle = startAngle + angle;
            float elapsedTime = 0f;

            duration *= 0.5f;

            while (!stopAnim && elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                handle.Rect.localEulerAngles = Vector3.Lerp(startAngle, endAngle, Mathf.Clamp01(elapsedTime / duration));

                yield return null;
            }

            handle.Rotate();

            startAngle = endAngle;
            endAngle = startAngle + angle;
            elapsedTime = 0f;

            while (!stopAnim && elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                handle.Rect.localEulerAngles = Vector3.Lerp(startAngle, endAngle, Mathf.Clamp01(elapsedTime / duration));

                yield return null;
            }

            handle.Rect.localEulerAngles = endAngle;
        }
        else handle.Rect.localEulerAngles += angle * 2;

        playCount--;
    }

    private IEnumerator FocusAnim(CharStandingHandle handle, Vector2 size, float duration)
    {
        playCount++;

        if (size.x == -1) size.x = handle.transform.localScale.x;
        if (size.y == -1) size.y = handle.transform.localScale.y;

        if (duration > 0)
        {
            Vector2 startPos = handle.transform.localScale;
            float elapsedTime = 0f;

            while (!stopAnim && elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                handle.transform.localScale = Vector2.Lerp(startPos, size, Mathf.Clamp01(elapsedTime / duration));

                yield return null;
            }
        }

        handle.transform.localScale = size;

        playCount--;
    }

    private IEnumerator OutAndInAnim(CharStandingHandle handle, StandingDisplayInfo spriteInfo, bool enterOther, Vector2 outPos, Vector2 inPos, Vector2 size, float duration)
    {
        playCount++;

        if (duration > 0) duration *= 0.5f;

        yield return MovePosAnim(handle, outPos, duration);

        if (enterOther) handle.Rect.anchoredPosition *= new Vector2(-1, 1);

        handle.SetInfo(spriteInfo);
        handle.transform.localScale = size;

        handle.Rect.anchoredPosition = new(handle.Rect.anchoredPosition.x, inPos.y);

        yield return MovePosAnim(handle, inPos, duration);

        playCount--;
    }

    private IEnumerator OutAndHideAnim(CharStandingHandle handle, StandingDisplayInfo spriteInfo, Vector2 outPos, float duration)
    {
        playCount++;

        handle.SetInfo(spriteInfo);

        yield return MovePosAnim(handle, outPos, duration);

        handle.Clear();

        playCount--;
    }
}

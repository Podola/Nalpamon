using System;
using System.Collections;
using UnityEngine;

public abstract class ActionFadeBase : MonoBehaviour
{
    public string id;

    public Action fadeInCallback;
    public Action fadeOutCallback;

    public YieldInstruction StartFadeIn() => StartCoroutine(FadeIn());
    public YieldInstruction StartFadeOut() => StartCoroutine(FadeOut());

    protected virtual void Start() => ActionFadeManager.Instance.AddBase(this);

    public virtual void OnEvent(string eventName)
    {

    }

    protected virtual IEnumerator FadeIn()
    {

        yield return null;
    }

    protected virtual IEnumerator FadeOut()
    {

        yield return null;
    }
}

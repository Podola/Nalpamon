using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class ActionFadeManager : Singleton<ActionFadeManager>
{
    private readonly Dictionary<string, ActionFadeBase> fadeDict = new();

    private const string ACTION_FADE_DEFAULT_NAME = "Default";

    private void Start()
    {
        DialogueRunner runner = FindFirstObjectByType<DialogueRunner>();

        runner.AddCommandHandler<string, float>("ActionFade", WaitFade);
        runner.AddCommandHandler<string>("ActionFadeIn", FadeIn);
        runner.AddCommandHandler<string>("ActionFadeOut", FadeOut);

        runner.AddCommandHandler<string, string>("ActionFadeEvent", SendEvent);
    }

    public void AddBase(ActionFadeBase actionFadeBase) => fadeDict.Add(actionFadeBase.id, actionFadeBase);

    public void RemoveBase(ActionFadeBase actionFadeBase) => fadeDict.Remove(actionFadeBase.id);

    public void StartFade(string id, float waitTime = 0, Action fadeInCallback = null, Action fadeOutCallback = null)
    {
        StartCoroutine(WaitFadeCallback(id, waitTime, fadeInCallback, fadeOutCallback));
    }

    public IEnumerator WaitFadeCallback(string id, float waitTime = 0, Action fadeInCallback = null, Action fadeOutCallback = null)
    {
        if (!fadeDict.ContainsKey(id)) id = ACTION_FADE_DEFAULT_NAME;

        yield return fadeDict[id].StartFadeIn();

        fadeInCallback?.Invoke();

        if (waitTime > 0) yield return new WaitForSeconds(waitTime);

        yield return fadeDict[id].StartFadeOut();

        fadeOutCallback?.Invoke();
    }

    public void SendEvent(string id, string eventName) => fadeDict[id].OnEvent(eventName);

    private IEnumerator WaitFade(string id, float duration = 0)
    {
        if (!fadeDict.ContainsKey(id)) id = ACTION_FADE_DEFAULT_NAME;

        yield return fadeDict[id].StartFadeIn();

        if (duration > 0) yield return new WaitForSeconds(duration);

        yield return fadeDict[id].StartFadeOut();
    }

    private IEnumerator FadeIn(string id)
    {
        if (!fadeDict.ContainsKey(id)) id = ACTION_FADE_DEFAULT_NAME;

        yield return fadeDict[id].StartFadeIn();
    }

    private IEnumerator FadeOut(string id)
    {
        if (!fadeDict.ContainsKey(id)) id = ACTION_FADE_DEFAULT_NAME;

        yield return fadeDict[id].StartFadeOut();
    }
}

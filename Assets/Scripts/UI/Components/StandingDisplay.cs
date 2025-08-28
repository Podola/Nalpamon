using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using Yarn.Markup;
using Yarn.Unity;

public class StandingDisplay : ActionMarkupHandler, IPresenterEvent
{
    [SerializeField] private RectTransform parent;

    [SerializeField] private CharStandingHandle prefab;
    [SerializeField] private int defaultCreateCount = 5;

    [Header("Rule Animator")]
    [SerializeField] private CharInfoSO[] mainChars;

    private StandingAnimator animator;
    private StandingRuleAnimator ruleAnimator;

    private readonly Dictionary<string, CharStandingHandle> standings = new();

    public CharInfoSO[] MainChars => mainChars;

    public IObjectPool<CharStandingHandle> Pool { get; private set; }

    private void Start()
    {
        Pool = new ObjectPool<CharStandingHandle>(CreateObject, OnGetObject, OnReleseObject, OnDestroyObject);

        CreateDefault();

        DialogueRunner runner = FindFirstObjectByType<DialogueRunner>();
        CustomLinePresenter lineView = FindFirstObjectByType<CustomLinePresenter>();

        LinePresenterHub hub = FindFirstObjectByType<LinePresenterHub>();

        animator = new(this);
        ruleAnimator = new(runner, this);

        hub.AddPresenterEvent(this);

        if (lineView != null) lineView.temporalProcessors.Add(this);

        runner.onDialogueStart.RemoveListener(OnDialogueStart);
        runner.onDialogueStart.AddListener(OnDialogueStart);

        runner.onDialogueComplete.RemoveListener(OnDialogueComplete);
        runner.onDialogueComplete.AddListener(OnDialogueComplete);

        runner.onNodeStart.RemoveListener(OnNodeStart);
        runner.onNodeStart.AddListener(OnNodeStart);

        runner.onNodeComplete.RemoveListener(OnNodeComplete);
        runner.onNodeComplete.AddListener(OnNodeComplete);

        AddCommandHandlers(runner);
    }

    public void OnDestroy()
    {
        DialogueRunner runner = FindFirstObjectByType<DialogueRunner>();
        CustomLinePresenter lineView = FindFirstObjectByType<CustomLinePresenter>();

        if (lineView != null) lineView.temporalProcessors.Remove(this);

        runner.onDialogueStart.RemoveListener(OnDialogueStart);

        runner.onDialogueComplete.RemoveListener(OnDialogueComplete);

        runner.onNodeStart.RemoveListener(OnNodeStart);

        runner.onNodeComplete.RemoveListener(OnNodeComplete);

        RemoveCommandHandlers(runner);
    }

    #region Yarn
    public void OnDialogueStart()
    {

    }

    public void OnDialogueComplete() => ClearStandings();

    public void OnNodeStart(string nadeName) => ruleAnimator.OnNodeStart(nadeName);

    public void OnNodeComplete(string nadeName)
    {

    }

    public override void OnPrepareForLine(MarkupParseResult line, TMP_Text text)
    {

    }

    public override void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
    {

    }

    public override YarnTask OnCharacterWillAppear(int currentCharacterIndex, MarkupParseResult line, CancellationToken cancellationToken)
    {
        foreach (MarkupAttribute attribute in line.Attributes)
        {
            switch (attribute.Name)
            {
                case "character":
                    if (attribute.TryGetProperty("name", out string name))
                    {
                        if (!standings.ContainsKey(name)) return YarnTask.CompletedTask;

                        standings[name].ChangeMouthShape(StandingMouthType.None);
                    }
                    break;
            }
        }

        return YarnTask.CompletedTask;
    }

    public override void OnLineDisplayComplete()
    {
        foreach (CharStandingHandle handle in standings.Values) handle.ChangeMouthShape(StandingMouthType.Close);
    }

    public override void OnLineWillDismiss() => OnLineDisplayComplete();
    #endregion

    private void ShowStanding(string charID, string standingID, bool lookRight, float duration, Vector4 posInfo, Vector2 size)
    {
        CharStandingHandle handle = GetHandle(charID);

        if (handle == null)
        {
            handle = Pool.Get();
            standings.Add(charID, handle);

            duration = 0;
        }

        if (standingID == "") standingID = handle.SpriteID;

        if (ChapterDataManager.Instance.StandingDict.TryGetValue(charID, out Dictionary<string, StandingSpriteInfo> standing) && standing.TryGetValue(standingID, out StandingSpriteInfo sprite))
        {
            animator.SetPos(handle, handle.GetConvertPosInfo(posInfo, parent), duration);
            animator.SetSize(handle, size, duration);

            handle.SetInfo(new(charID, -1, sprite, lookRight));
        }
        else Debug.LogError($"[DialogueManager] Cannot found standing Sprite. ID: '{charID} - {standingID}'");
    }

    private void ShowIn(string charID, string standingID, bool lookRight, bool inRight, float duration, Vector4 posInfo, Vector2 size)
    {
        CharStandingHandle handle = GetHandle(charID);

        if (handle == null)
        {
            handle = Pool.Get();
            standings.Add(charID, handle);

            float splitCount = Screen.width / handle.Rect.sizeDelta.x;

            Vector4 inPosInfo = new(splitCount, inRight ? splitCount : -1, posInfo.z, posInfo.w);

            animator.SetPos(handle, handle.GetConvertPosInfo(inPosInfo, parent));
            animator.SetSize(handle, size, 0);
        }

        if (standingID == "") standingID = handle.SpriteID;

        if (ChapterDataManager.Instance.StandingDict.TryGetValue(charID, out Dictionary<string, StandingSpriteInfo> standing) && standing.TryGetValue(standingID, out StandingSpriteInfo sprite))
        {
            animator.SetPos(handle, handle.GetConvertPosInfo(posInfo, parent), duration);
            animator.SetSize(handle, size, duration);

            handle.SetInfo(new(charID, -1, sprite, lookRight));
        }
        else Debug.LogError($"[DialogueManager] Cannot found standing Sprite. ID: '{charID} - {standingID}'");
    }

    private void HideOut(string charID, string standingID, bool lookRight, bool outRight, float duration)
    {
        CharStandingHandle handle = GetHandle(charID);

        if (handle == null) return;

        if (standingID == "") standingID = handle.SpriteID;

        if (ChapterDataManager.Instance.StandingDict.TryGetValue(charID, out Dictionary<string, StandingSpriteInfo> standing) && standing.TryGetValue(standingID, out StandingSpriteInfo sprite))
        {
            float splitCount = Screen.width / handle.Rect.sizeDelta.x;

            StandingDisplayInfo spriteInfo = new(charID, -1, sprite, lookRight);
            Vector4 outPosInfo = new(splitCount, outRight ? splitCount : -1, 0, 0);

            animator.OutAndHide(handle, spriteInfo, handle.GetConvertPosInfo(outPosInfo, parent), duration);
        }
        else Debug.LogError($"[DialogueManager] Cannot found standing Sprite. ID: '{charID} - {standingID}'");
    }

    private void OutAndIn(string charID, string standingID, bool lookRight, bool outRight, bool inRight, float duration, Vector4 posInfo, Vector2 size)
    {
        CharStandingHandle handle = GetHandle(charID);

        if (handle == null) return;

        if (standingID == "") standingID = handle.SpriteID;

        if (ChapterDataManager.Instance.StandingDict.TryGetValue(charID, out Dictionary<string, StandingSpriteInfo> standing) && standing.TryGetValue(standingID, out StandingSpriteInfo sprite))
        {
            float splitCount = Screen.width / handle.Rect.sizeDelta.x;

            StandingDisplayInfo spriteInfo = new(charID, -1, sprite, lookRight);
            Vector4 outPosInfo = new(splitCount, outRight ? splitCount : -1, 0, 0);

            if (outRight == inRight) animator.OutAndInSame(handle, spriteInfo, handle.GetConvertPosInfo(outPosInfo, parent), handle.GetConvertPosInfo(posInfo, parent), size, duration);
            else animator.OutAndInOther(handle, spriteInfo, handle.GetConvertPosInfo(outPosInfo, parent), handle.GetConvertPosInfo(posInfo, parent), size, duration);
        }
        else Debug.LogError($"[DialogueManager] Cannot found standing Sprite. ID: '{charID} - {standingID}'");
    }

    public void LineEvent(LocalizedLine line)
    {
        ruleAnimator.OnLineEvent(line);
    }

    public async YarnTask LineWait(LineCancellationToken token)
    {
        animator.Play();

        await YarnTask.WaitUntil(() => animator.IsComplete, token.HurryUpToken);
    }

    #region Convert
    public CharStandingHandle GetHandle(string charID)
    {
        if (standings.ContainsKey(charID)) return standings[charID];

        return null;
    }

    private Vector4 StringPosToFloatPosX(string posX, string posY = "")
    {
        Vector4 result = new(0, 0, 2, -1.5f);

        switch (posX)
        {
            case "":
                break;

            case "left1":
                result.x = 5;
                result.y = 0.5f;
                    break;

            case "left2":
                result.x = 5;
                result.y = 1f;
                    break;

            case "center":
                result.x = 5;
                result.y = 2f;
                    break;

            case "right2":
                result.x = 5;
                result.y = 3f;
                    break;

            case "right1":
                result.x = 5;
                result.y = 3.5f;
                    break;

            default:
                string[] positions = posX.Split('/');

                if (positions.Length == 2)
                {
                    float.TryParse(positions[0], out result.x);
                    float.TryParse(positions[1], out result.y);
                }
                else throw new NotImplementedException($"[DialogueManager] Invalid position: '{posX}'");
                break;
        }

        switch (posY)
        {
            case "":
                break;

            case "up":
                result.z = 5;
                result.w = 1f;
                    break;

            case "center":
                result.z = 3;
                result.w = 0.15f;
                    break;

            case "down":
                result.z = 5;
                result.w = -1f;
                    break;

            default:
                string[] positions = posY.Split('/');

                if (positions.Length == 2)
                {
                    float.TryParse(positions[0], out result.z);
                    float.TryParse(positions[1], out result.w);
                }
                else throw new NotImplementedException($"[DialogueManager] Invalid position: '{posY}'");
                break;
        }

        return result;
}
    #endregion

    #region Standing Commands
    private void AddCommandHandlers(DialogueRunner runner)
    {
        runner.AddCommandHandler("ForceStandingAnimations", animator.PlayForce);

        runner.AddCommandHandler<string, int>("FixSiblingIndex", FixSiblingIndex);

        runner.AddCommandHandler<string, bool, float>("Rotate", Rotate);

        runner.AddCommandHandler<string, string, bool, float, string, string, float>("ShowOrMove", ShowOrMove);

        runner.AddCommandHandler<string, string, bool, float, string, string, float>("LeftIn", LeftIn);
        runner.AddCommandHandler<string, string, bool, float, string, string, float>("RightIn", RightIn);

        runner.AddCommandHandler<string, string, bool, float>("LeftOut", LeftOut);
        runner.AddCommandHandler<string, string, bool, float>("RightOut", RightOut);

        runner.AddCommandHandler<string, string, bool, float, string, string, float>("LeftOutLeftIn", LeftOutLeftIn);
        runner.AddCommandHandler<string, string, bool, float, string, string, float>("RightOutRightIn", RightOutRightIn);
        runner.AddCommandHandler<string, string, bool, float, string, string, float>("LeftOutRightIn", LeftOutRightIn);
        runner.AddCommandHandler<string, string, bool, float, string, string, float>("RightOutLeftIn", RightOutLeftIn);

        runner.AddCommandHandler<string, string, string, string, float>("ChangePosition", ChangePosition);
        runner.AddCommandHandler<string, string, string, string, float>("RightOutNewLeftIn", RightOutNewLeftIn);

        runner.AddCommandHandler<string, string, bool, string, string, float>("ShowStanding", ShowStanding);
        runner.AddCommandHandler<string>("HideStanding", HideStanding);
        runner.AddCommandHandler("ClearStandings", ClearStandings);

        runner.AddCommandHandler<string, string, string>("ChangeRuleType", ruleAnimator.ChangeRuleType);
    }

    private void RemoveCommandHandlers(DialogueRunner runner)
    {
        runner.RemoveCommandHandler("ForceStandingAnimations");

        runner.RemoveCommandHandler("FixSiblingIndex");

        runner.RemoveCommandHandler("Rotate");

        runner.RemoveCommandHandler("ShowOrMove");

        runner.RemoveCommandHandler("LeftIn");
        runner.RemoveCommandHandler("RightIn");

        runner.RemoveCommandHandler("LeftOut");
        runner.RemoveCommandHandler("RightOut");

        runner.RemoveCommandHandler("LeftOutLeftIn");
        runner.RemoveCommandHandler("RightOutRightIn");
        runner.RemoveCommandHandler("LeftOutRightIn");
        runner.RemoveCommandHandler("RightOutLeftIn");

        runner.RemoveCommandHandler("ChangePosition");
        runner.RemoveCommandHandler("RightOutNewLeftIn");

        runner.RemoveCommandHandler("ShowStanding");
        runner.RemoveCommandHandler("HideStanding");
        runner.RemoveCommandHandler("ClearStandings");

        runner.RemoveCommandHandler("ChangeRuleType");
    }

    public void Play() => animator.Play();

    public void FixSiblingIndex(string charID, int sibling)
    {
        CharStandingHandle handle = GetHandle(charID);

        if (handle == null) return;

        handle.Sibling = sibling;
    }

    public void Rotate(string charID, bool right, float duration)
    {
        CharStandingHandle handle = GetHandle(charID);

        if (handle == null) return;

        animator.SetAngle(handle, right, duration);
    }

    public void ChangeStanding(string charID, string standingID)
    {
        CharStandingHandle handle = GetHandle(charID);

        if (handle == null) return;

        if (ChapterDataManager.Instance.StandingDict.TryGetValue(charID, out Dictionary<string, StandingSpriteInfo> standing) && standing.TryGetValue(standingID, out StandingSpriteInfo sprite))
        {
            handle.SetInfo(new(charID, handle.Sibling, sprite, handle.IsRight));
        }
        else Debug.LogError($"[DialogueManager] Cannot found standing Sprite. ID: '{charID} - {standingID}'");
    }

    public void ShowStanding(string charID, string standingID, bool lookRight, string posX, string posY = "", float size = -1)
    {
        ShowStanding(charID, standingID, lookRight, 0.3f, StringPosToFloatPosX(posX, posY), new(size, size));
    }

    public void ShowOrMove(string charID, string standingID, bool lookRight, float duration, string posX, string posY = "", float size = -1)
    {
        ShowStanding(charID, standingID, lookRight, duration, StringPosToFloatPosX(posX, posY), new(size, size));
    }

    public void LeftIn(string charID, string standingID, bool lookRight, float duration, string posX, string posY = "", float size = -1)
    {
        ShowIn(charID, standingID, lookRight, false, duration, StringPosToFloatPosX(posX, posY), new(size, size));
    }

    public void RightIn(string charID, string standingID, bool lookRight, float duration, string posX, string posY = "", float size = -1)
    {
        ShowIn(charID, standingID, lookRight, true, duration, StringPosToFloatPosX(posX, posY), new(size, size));
    }

    public void LeftOut(string charID, string standingID, bool lookRight, float duration)
    {
        HideOut(charID, standingID, lookRight, false, duration);
    }

    public void RightOut(string charID, string standingID, bool lookRight, float duration)
    {
        HideOut(charID, standingID, lookRight, true, duration);
    }

    public void LeftOutRightIn(string charID, string standingID, bool lookRight, float duration, string posX, string posY = "", float size = -1)
    {
        OutAndIn(charID, standingID, lookRight, false, true, duration, StringPosToFloatPosX(posX, posY), new(size, size));
    }

    public void RightOutLeftIn(string charID, string standingID, bool lookRight, float duration, string posX, string posY = "", float size = -1)
    {
        OutAndIn(charID, standingID, lookRight, true, false, duration, StringPosToFloatPosX(posX, posY), new(size, size));
    }

    public void LeftOutLeftIn(string charID, string standingID, bool lookRight, float duration, string posX, string posY = "", float size = -1)
    {
        OutAndIn(charID, standingID, lookRight, false, false, duration, StringPosToFloatPosX(posX, posY), new(size, size));
    }

    public void RightOutRightIn(string charID, string standingID, bool lookRight, float duration, string posX, string posY = "", float size = -1)
    {
        OutAndIn(charID, standingID, lookRight, true, true, duration, StringPosToFloatPosX(posX, posY), new(size, size));
    }

    public void ChangePosition(string charID1, string standingID1, string charID2, string standingID2, float duration)
    {
        CharStandingHandle handle1 = GetHandle(charID1);
        CharStandingHandle handle2 = GetHandle(charID2);

        if (handle1 == null || handle2 == null) return;

        Vector2 posX1 = handle1.Rect.anchoredPosition;
        Vector2 posX2 = handle2.Rect.anchoredPosition;

        if (ChapterDataManager.Instance.StandingDict.TryGetValue(charID1, out Dictionary<string, StandingSpriteInfo> standing1) && standing1.TryGetValue(standingID1, out StandingSpriteInfo sprite1))
        {
            if (ChapterDataManager.Instance.StandingDict.TryGetValue(charID2, out Dictionary<string, StandingSpriteInfo> standing2) && standing2.TryGetValue(standingID2, out StandingSpriteInfo sprite2))
            {
                float splitCount1 = Screen.width / handle1.Rect.sizeDelta.x;
                float splitCount2 = Screen.width / handle2.Rect.sizeDelta.x;

                StandingDisplayInfo spriteInfo1 = new(charID1, handle2.Sibling, sprite1, !handle1.IsRight);
                StandingDisplayInfo spriteInfo2 = new(charID2, handle1.Sibling, sprite2, !handle2.IsRight);

                Vector4 outPosInfo1 = new(splitCount1, !handle1.IsRight ? splitCount1 : -1, 0, 0);
                Vector4 outPosInfo2 = new(splitCount2, !handle2.IsRight ? splitCount2 : -1, 0, 0);

                animator.OutAndInSame(handle1, spriteInfo1, handle1.GetConvertPosInfo(outPosInfo1, parent), posX2, handle2.transform.localScale, duration);
                animator.OutAndInSame(handle2, spriteInfo2, handle2.GetConvertPosInfo(outPosInfo2, parent), posX1, handle1.transform.localScale, duration);
            }
            else Debug.LogError($"[DialogueManager] Cannot found standing Sprite. ID: '{charID2} - {standingID2}'");
        }
        else Debug.LogError($"[DialogueManager] Cannot found standing Sprite. ID: '{charID1} - {standingID1}'");
    }

    public void RightOutNewLeftIn(string charID1, string standingID1, string charID2, string standingID2, float duration)
    {
        CharStandingHandle handle = GetHandle(charID1);

        if (handle == null) return;

        float ratioX = handle.Rect.anchoredPosition.x / Screen.width;
        float ratioY = handle.Rect.anchoredPosition.y / Screen.height;

        RightOut(charID1, standingID1, handle.IsRight, duration);
        ShowIn(charID2, standingID2, handle.IsRight, false, duration, new Vector4(1, ratioX, 1, ratioY), new Vector2(handle.transform.localScale.x, handle.transform.localScale.x));

        FixSiblingIndex(charID2, handle.Sibling);
    }

    public void HideStanding(string charID)
    {
        if (standings.ContainsKey(charID)) standings[charID].Clear();
    }

    public void ClearStandings()
    {
        foreach (CharStandingHandle handle in standings.Values) handle.Clear();

        ruleAnimator.Clear();
    }
    #endregion

    #region 풀링
    /// <summary>
    /// defaultCreateCount만큼 오브젝트 생성
    /// </summary>
    public void CreateDefault()
    {
        if (prefab == null) return;

        for (int i = 0; i < defaultCreateCount; i++) Pool.Release(CreateObject());
    }

    private CharStandingHandle CreateObject()
    {
        CharStandingHandle handle = Instantiate(prefab, parent);

        handle.SetPool(Pool);

        return handle;
    }

    private void OnGetObject(CharStandingHandle handle) => handle.gameObject.SetActive(true);

    private void OnReleseObject(CharStandingHandle handle)
    {
        if (this == null) OnDestroyObject(handle);
        else
        {
            if (!string.IsNullOrEmpty(handle.CharID)) standings.Remove(handle.CharID);

            handle.gameObject.SetActive(false);
        }
    }

    private void OnDestroyObject(CharStandingHandle handle)
    {
        if (this != null && !string.IsNullOrEmpty(handle.CharID)) standings.Remove(handle.CharID);

        Destroy(handle.gameObject);
    }
    #endregion
}

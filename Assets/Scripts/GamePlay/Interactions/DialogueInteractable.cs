using UnityEngine;
using UnityEngine.Events;
using static Util;
using InteractTask = StepDatabaseSO.InteractTask;

/// <summary>
/// 대화 기능이 있는 모든 상호작용 객체(NPC, 사물)의 기반이 되는 추상 클래스입니다.
/// 아이콘 관리, 상태 변경 감지, 대화 재생 로직 등을 공통으로 처리합니다.
/// </summary>
public abstract class DialogueInteractable : MonoBehaviour, IInteractable
{
    private System.Action _dmStartHandler;
    private System.Action _dmCompleteHandler;

    public enum Subject { NPC, PROP }

    [Header("Target Identification")]
    [Tooltip("이 객체의 종류 (NPC 또는 PROP)")]
    public Subject InteractSubject = Subject.NPC;
    [Tooltip("게임 내에서 사용할 고유 이름. 비워두면 GameObject 이름을 사용합니다.")]
    public string NameOverride;

    [Header("Icons")]
    public GameObject IconParent;
    public GameObject IconBefore;
    public GameObject IconRepeat;
    public GameObject IconBlocked;
    [Tooltip("대화 노드가 존재할 때만 아이콘을 표시할지 여부")]
    public bool showIconOnlyWhenNodeExists = true;

    [Header("Events")]
    public UnityEvent OnInteractStarted;
    public UnityEvent OnInteractCompleted;

    // 자식 클래스에서 설정할 내부 옵션들
    [HideInInspector] public bool useDayPatternForNPC = false;
    [HideInInspector] public bool useStepPatternForNPC = false;
    [HideInInspector] public bool isPartyMember = false;
    [HideInInspector] public string nodeBaseOverride;
    [HideInInspector] public string repeatNodeOverride;
    [HideInInspector] public bool markAfterDialogue = true;

    protected string TargetName => string.IsNullOrEmpty(NameOverride) ? gameObject.name : NameOverride;
    protected MarkType DoneType => (InteractSubject == Subject.NPC) ? MarkType.NPC_TALKED : MarkType.PROP_INSPECTED;

    protected virtual void OnEnable()
    {
        // 게임 상태 변경 이벤트를 구독하여 아이콘을 자동으로 갱신
        GameEvents.OnStepChanged += HandleStepChanged;
        GameEvents.OnMarkChanged += HandleMarkChanged;
        ShowIcon(false);
    }

    protected virtual void OnDisable()
    {
        GameEvents.OnStepChanged -= HandleStepChanged;
        GameEvents.OnMarkChanged -= HandleMarkChanged;
    }

    // 자식 클래스에서 구체적인 상호작용 로직을 구현
    public abstract void Interact(Transform caller);
    public abstract void UpdateIcon();

    private void HandleStepChanged(StepId prev, StepId now) => UpdateIcon();
    private void HandleMarkChanged(string key, bool value) => UpdateIcon();

    public virtual void ShowIcon(bool on)
    {
        if (IconParent == null) return;

        if (InputManager.Instance != null && InputManager.Instance.IsGloballyLocked)
        {
            IconParent.SetActive(false);
            return;
        }

        IconParent.SetActive(on);
    }

    /// <summary>
    /// 아이콘의 종류(대화 가능, 반복, 불가)를 설정합니다.
    /// </summary>
    protected void SetIcon(bool parent, bool before, bool repeat, bool blocked)
    {
        if (IconParent && IconParent.activeSelf)
        {
            if (IconBefore) IconBefore.SetActive(before);
            if (IconRepeat) IconRepeat.SetActive(repeat);
            if (IconBlocked) IconBlocked.SetActive(blocked);
        }
    }

    /// <summary>
    /// 대화를 재생하고, 재생 전후에 필요한 처리를 합니다.
    /// </summary>
    /// <param name="node">재생할 Yarn 노드 이름</param>
    /// <param name="caller">상호작용을 요청한 객체 (주로 플레이어)</param>
    /// <param name="after">대화 종료 후 실행할 콜백</param>
    protected void Play(string node, Transform caller, System.Action after)
    {
        if (!NodeExists(node, true)) { after?.Invoke(); return; }

        System.Action restoreFacing = null;
        if (InteractSubject == Subject.NPC && caller != null)
        {
            var facingHandler = GetComponent<NPCDialogueFacing>();
            if (facingHandler != null)
                restoreFacing = facingHandler.ApplyFacingBeforeDialogue(caller);
        }

        var dialogueManager = DialogueManager.Instance;
        if (dialogueManager == null)
        {
            after?.Invoke();
            return;
        }

        // UnityAction으로 콜백을 한번 감싸서 등록합니다.
        UnityEngine.Events.UnityAction onComplete = null;
        onComplete = () =>
        {
            dialogueManager.Runner.onDialogueComplete.RemoveListener(onComplete);
            restoreFacing?.Invoke();
            after?.Invoke();
        };

        dialogueManager.Runner.onDialogueComplete.AddListener(onComplete);
        dialogueManager.Runner.StartDialogue(node);
    }

    protected bool IsFreeStep()
    {
        var cfg = StepManager.Instance?.GetConfig(StepManager.Instance.CurrentStep);
        return cfg != null && cfg.stepMode == StepDatabaseSO.StepMode.Free;
    }

    protected string GlobalFallbackRepeat()
    {
        return StepManager.Instance?.Database?.fallbackRepeatNode;
    }
}
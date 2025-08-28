using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using static Util;

/// <summary>
/// 특정 조건을 만족해야만 통과할 수 있는 물리적인 관문(Gate) 트리거입니다.
/// ConditionSO 목록을 통해 매우 유연한 통과 조건을 설정할 수 있습니다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class StepGateTrigger : MonoBehaviour
{
    [Header("Activation Scope")]
    [Tooltip("이 게이트가 활성화될 스텝")]
    public StepId requiredCurrentStep;
    [Tooltip("true이면 requiredCurrentStep일 때만 트리거가 자동으로 활성화됩니다.")]
    public bool autoEnableByStep = true;

    [Header("Success Conditions")]
    [Tooltip("이 게이트를 통과하기 위해 충족해야 할 모든 조건 목록")]
    public List<ConditionSO> conditionsToSucceed;

    [Header("Dialogue Feedback")]
    [Tooltip("성공 시 재생할 Yarn 노드가 있을 경우 true로 설정")]
    public bool playSuccessNode = true;
    [Tooltip("실패 시 재생할 Yarn 노드가 있을 경우 true로 설정")]
    public bool playFailNode = true;

    [Header("Events")]
    public UnityEvent OnTriggered;
    public UnityEvent OnSuccessEvent;
    public UnityEvent OnFailEvent;

    private Collider2D _col;

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
        _col.isTrigger = true;
    }

    private void OnEnable()
    {
        GameEvents.OnStepChanged += HandleStepChanged;
        ApplyStepActive();
    }

    private void OnDisable()
    {
        GameEvents.OnStepChanged -= HandleStepChanged;
    }

    void HandleStepChanged(StepId prev, StepId next) => ApplyStepActive();

    void ApplyStepActive()
    {
        if (!autoEnableByStep || _col == null) return;
        bool shouldBeActive = (StepManager.Instance?.CurrentStep == requiredCurrentStep);
        if (_col.enabled != shouldBeActive)
        {
            _col.enabled = shouldBeActive;
            Log.V($"[{gameObject.name}] StepGateTrigger 활성 상태 변경 -> {shouldBeActive}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TryTrigger();
        }
    }

    void TryTrigger()
    {
        var mgr = StepManager.Instance;
        if (mgr == null || mgr.CurrentStep != requiredCurrentStep) return;

        var dm = DialogueManager.Instance;
        if (dm != null && dm.IsRunning) return;

        Log.V($"[{gameObject.name}] 게이트 트리거 시도.");
        OnTriggered?.Invoke();

        if (AreConditionsMet())
        {
            Log.I($"[{gameObject.name}] 게이트 통과 성공.");
            OnSuccessEvent?.Invoke();

            if (playSuccessNode)
            {
                PlayNodeAndSetMark(GateSuccessNode(mgr.CurrentStep), MarkGateSucceeded(mgr.CurrentStep), true);
            }
            else
            {
                mgr.SetMark(MarkGateSucceeded(mgr.CurrentStep), true, "GateSuccess");
            }
        }
        else
        {
            var unmetConditions = conditionsToSucceed.Where(c => c != null && !c.IsMet()).Select(c => c.name).ToList();
            string reason = unmetConditions.Any() ? string.Join(", ", unmetConditions) : "조건 없음";
            Log.I($"[{gameObject.name}] 게이트 통과 실패. 미충족 조건: {reason}");

            OnFailEvent?.Invoke();
            if (playFailNode)
            {
                string failNode = GateFailNode(mgr.CurrentStep);
                if (NodeExists(failNode, false))
                {
                    DialogueManager.StartDialogue(failNode);
                }
            }
        }
    }

    private bool AreConditionsMet()
    {
        if (conditionsToSucceed == null || conditionsToSucceed.Count == 0) return true;
        return conditionsToSucceed.All(c => c != null && c.IsMet());
    }

    private void PlayNodeAndSetMark(string nodeName, string markToSet, bool value)
    {
        var mgr = StepManager.Instance;
        if (NodeExists(nodeName, false))
        {
            UnityEngine.Events.UnityAction onComplete = null;
            onComplete = () =>
            {
                DialogueManager.Instance.Runner.onDialogueComplete.RemoveListener(onComplete);
                mgr.SetMark(markToSet, value, "GateSuccessAfterDialogue");
            };
            DialogueManager.Instance.Runner.onDialogueComplete.AddListener(onComplete);
            DialogueManager.StartDialogue(nodeName);
        }
        else
        {
            mgr.SetMark(markToSet, value, "GateSuccess");
        }
    }
}
using UnityEngine;
using static Util;
using InteractTask = StepDatabaseSO.InteractTask;

/// <summary>
/// NPC와의 상호작용을 구체적으로 처리하는 최종 개선 버전 클래스입니다.
/// Task, Step-Specific, Daily, Repeat 등 모든 대화 유형을 명확한 우선순위에 따라 처리합니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class NPCInteractable : DialogueInteractable
{
    void Reset()
    {
        InteractSubject = Subject.NPC;
        showIconOnlyWhenNodeExists = true;
        markAfterDialogue = true;
    }

    /// <summary>
    /// 플레이어가 상호작용을 시도할 때 호출되는 메인 로직입니다.
    /// </summary>
    public override void Interact(Transform caller)
    {
        var mgr = StepManager.Instance;
        if (mgr == null || (DialogueManager.Instance != null && DialogueManager.Instance.IsRunning)) return;

        // 현재 상황에서 재생할 가장 적절한 Yarn 노드를 찾습니다.
        string nodeToPlay = ResolveDialogueNode(out var afterAction);

        if (!string.IsNullOrEmpty(nodeToPlay))
        {
            Play(nodeToPlay, caller, afterAction);
        }
    }

    /// <summary>
    /// 현재 게임 상태에 맞춰 상호작용 아이콘을 갱신합니다.
    /// </summary>
    public override void UpdateIcon()
    {
        var mgr = StepManager.Instance;
        if (mgr == null) { ShowIcon(false); return; }

        // 1. 필수 과업(Task) 상태 확인
        bool allowed = mgr.CanInteractNow(MarkType.NPC_TALKED, TargetName, out InteractTask expected, out bool eligible);
        if (!allowed)
        {
            SetIcon(true, false, false, true); // Blocked
            return;
        }
        if (expected != null)
        {
            SetIcon(true, eligible, false, !eligible); // Before or Blocked
            return;
        }

        // 2. 과업이 없을 때, 재생 가능한 대화 유형에 따라 아이콘 결정
        string node = ResolveDialogueNode(out _);
        if (showIconOnlyWhenNodeExists && string.IsNullOrEmpty(node))
        {
            ShowIcon(false);
            return;
        }

        // 아이콘 종류 결정: 반복 노드가 재생될 것이라면 Repeat, 아니면 First
        bool isRepeat = node.EndsWith("_Repeat") || node == GlobalFallbackRepeat();
        SetIcon(true, !isRepeat, isRepeat, false);
    }

    /// <summary>
    /// 현재 상태에서 재생해야 할 가장 우선순위가 높은 Yarn 노드 이름을 결정하고,
    /// 대화 후 실행해야 할 액션(마크 기록 등)을 반환합니다.
    /// </summary>
    private string ResolveDialogueNode(out System.Action afterAction)
    {
        var mgr = StepManager.Instance;
        afterAction = null;

        // --- 우선순위 1: 필수 과업(Task) ---
        var pendingTask = mgr.FindPendingTask(MarkType.NPC_TALKED, TargetName);
        if (pendingTask != null)
        {
            string baseName = !string.IsNullOrEmpty(pendingTask.nodeBaseOverride) ? pendingTask.nodeBaseOverride : TargetName;
            afterAction = () => mgr.TryCompleteTask(MarkType.NPC_TALKED, TargetName, "TaskAfterDialogue");
            return NodeExists($"{baseName}_{mgr.CurrentStep}", false) ? $"{baseName}_{mgr.CurrentStep}" : baseName;
        }

        var step = mgr.CurrentStep;
        var day = DayOf(step);
        string stepFirstKey = $"Mark_SYS_NPCStepFirst_{TargetName}_{step}";
        string dailyFirstKey = MarkDaily(MarkType.NPC_TALKED, TargetName);

        // --- 우선순위 2: 스텝-한정 '최초' 대화 ---
        string stepFirstNode = $"{TargetName}_{step}_First";
        if (NodeExists(stepFirstNode, false) && !mgr.GetMark(stepFirstKey))
        {
            afterAction = () => mgr.SetMark(stepFirstKey, true, "NPCStepFirst");
            return stepFirstNode;
        }

        // --- 우선순위 3: 일일 '최초' 대화 ---
        string dailyFirstNode = $"{TargetName}_{day}_First";
        if (NodeExists(dailyFirstNode, false) && !mgr.GetMark(dailyFirstKey))
        {
            afterAction = () => mgr.SetDaily(MarkType.NPC_TALKED, TargetName, true, "DailyFirst");
            return dailyFirstNode;
        }

        string globalFirstNode = $"{TargetName}_First";
        if (NodeExists(globalFirstNode, false) && !mgr.GetMark(dailyFirstKey))
        {
            afterAction = () => mgr.SetDaily(MarkType.NPC_TALKED, TargetName, true, "DailyFirst");
            return globalFirstNode;
        }

        // --- 우선순위 4: 반복 대화 (가장 구체적인 것부터) ---
        return FindFirstAvailableNode(
            $"{TargetName}_{step}_Repeat",
            $"{TargetName}_{day}_Repeat",
            isPartyMember ? $"Room_{RoomManager.Instance.CurrentRoomType}_{TargetName}_Repeat" : null,
            $"{TargetName}_Repeat",
            IsFreeStep() ? GlobalFallbackRepeat() : null
        );
    }

    /// <summary>
    /// 주어진 노드 이름 목록 중에서 존재하는 첫 번째 노드 이름을 반환합니다.
    /// </summary>
    private string FindFirstAvailableNode(params string[] nodeNames)
    {
        foreach (var name in nodeNames)
        {
            if (!string.IsNullOrEmpty(name) && NodeExists(name, false))
            {
                return name;
            }
        }
        return null;
    }
}
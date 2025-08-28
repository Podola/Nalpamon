using UnityEngine;
using static Util;
using InteractTask = StepDatabaseSO.InteractTask;

/// <summary>
/// 조사 가능한 사물(Prop)과의 상호작용을 처리하는 최종 개선 버전 클래스입니다.
/// Task 여부와 Yarn 노드 존재 유무에 따라 모든 상호작용을 명확한 규칙으로 처리합니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class PropInteractable : DialogueInteractable
{
    [Header("Prop Restriction")]
    [Tooltip("이 사물이 상호작용 가능해지는 최소 스텝. 'None'이면 항상 활성화됩니다.")]
    public StepId requiredStep = StepId.None;

    void Reset()
    {
        InteractSubject = Subject.PROP;
        showIconOnlyWhenNodeExists = true;
        markAfterDialogue = true;
    }

    /// <summary>
    /// 현재 이 사물과 상호작용이 가능한지 여부를 확인합니다.
    /// </summary>
    private bool IsInteractionAllowedNow()
    {
        var mgr = StepManager.Instance;
        if (mgr == null) return false;

        // requiredStep이 설정되어 있고, 아직 해당 스텝에 도달하지 못했다면 비활성화
        if (requiredStep != StepId.None && mgr.CurrentStep < requiredStep)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 플레이어가 상호작용을 시도할 때 호출되는 메인 로직입니다.
    /// </summary>
    public override void Interact(Transform caller)
    {
        if (!IsInteractionAllowedNow()) return;

        var mgr = StepManager.Instance;
        if (mgr == null || (DialogueManager.Instance != null && DialogueManager.Instance.IsRunning)) return;

        // 1. 필수 과업(Task)이 있는지 확인하고 수행
        bool allowed = mgr.CanInteractNow(MarkType.PROP_INSPECTED, TargetName, out InteractTask expected, out bool eligible);
        if (expected != null && eligible)
        {
            string baseName = !string.IsNullOrEmpty(expected.nodeBaseOverride) ? expected.nodeBaseOverride : TargetName;
            string stepNode = $"{baseName}_{mgr.CurrentStep}";
            string node = NodeExists(stepNode, false) ? stepNode : baseName;
            Play(node, caller, () => mgr.TryCompleteTask(MarkType.PROP_INSPECTED, TargetName, "TaskAfterDialogue"));
            return;
        }

        // 2. 과업이 없거나 조건 미충족 시, 반복 대사 재생
        TryPlayRepeat(caller);
    }

    /// <summary>
    /// 현재 게임 상태에 맞춰 상호작용 아이콘의 모양을 결정합니다.
    /// 아이콘을 화면에 표시할지 여부는 PlayerController가 결정합니다.
    /// </summary>
    public override void UpdateIcon()
    {
        var mgr = StepManager.Instance;
        if (mgr == null || !IsInteractionAllowedNow())
        {
            // 상호작용 자체가 불가능하면 아이콘을 '차단됨' 상태로 두고 PlayerController가 끄도록 합니다.
            SetIcon(true, false, false, true);
            return;
        }

        // 1. 필수 과업(Task) 상태에 따라 아이콘 모양 결정
        bool allowed = mgr.CanInteractNow(MarkType.PROP_INSPECTED, TargetName, out InteractTask expected, out bool eligible);
        if (expected != null)
        {
            SetIcon(true, eligible, false, !eligible); // Before 또는 Blocked
            return;
        }

        // 2. 과업이 없을 경우, 반복 대사가 있을 때만 Repeat 아이콘 모양으로 설정
        if (HasRepeatNode())
        {
            SetIcon(true, false, true, false); // Repeat
        }
        else
        {
            // 보여줄 아이콘이 없으므로 모든 아이콘 상태를 끕니다.
            SetIcon(false, false, false, false);
        }
    }

    /// <summary>
    /// 재생 가능한 반복 대사가 있는지 확인합니다.
    /// </summary>
    private bool HasRepeatNode() => !string.IsNullOrEmpty(ResolveRepeatNode());

    /// <summary>
    /// 재생 가능한 반복 대사를 찾아 재생을 시도합니다.
    /// </summary>
    private void TryPlayRepeat(Transform caller)
    {
        string node = ResolveRepeatNode();
        if (!string.IsNullOrEmpty(node))
        {
            Play(node, caller, null);
        }
    }

    /// <summary>
    /// 현재 상태에서 재생할 가장 우선순위가 높은 반복 대사 노드를 찾습니다.
    /// </summary>
    private string ResolveRepeatNode()
    {
        var mgr = StepManager.Instance;
        if (mgr == null) return null;

        string baseName = TargetName;
        // Prop은 NPC와 달리, 스텝/날짜별 '최초' 대사 없이 바로 반복 대사로 넘어갑니다.
        // 이는 '중요한 이야기는 모두 Task'라는 Prop의 특성을 반영합니다.
        return FindFirstAvailableNode(
            $"{baseName}_{mgr.CurrentStep}_Repeat",
            $"{baseName}_{DayOf(mgr.CurrentStep)}_Repeat",
            $"{baseName}_Repeat",
            GlobalFallbackRepeat()
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
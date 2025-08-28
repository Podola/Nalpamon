using UnityEngine;

/// <summary>
/// 특정 '마크(Mark)'의 상태가 원하는 값인지 확인하는 조건입니다.
/// </summary>
[CreateAssetMenu(fileName = "MarkCondition", menuName = "Game/Conditions/Mark Condition")]
public class MarkConditionSO : ConditionSO
{
    [Tooltip("확인할 마크의 고유 키")]
    public string markKey;
    [Tooltip("필요한 마크의 값 (true 또는 false)")]
    public bool requiredValue = true;

    public override bool IsMet()
    {
        if (StepManager.Instance == null) return false;
        return StepManager.Instance.GetMark(markKey) == requiredValue;
    }
}
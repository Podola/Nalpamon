using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 여러 조건을 'AND' 논리로 묶습니다. 모든 하위 조건이 충족되어야만 IsMet이 true를 반환합니다.
/// </summary>
[CreateAssetMenu(fileName = "AndCondition", menuName = "Game/Conditions/And Condition")]
public class AndConditionSO : ConditionSO
{
    [Tooltip("모두 충족되어야 하는 하위 조건 목록")]
    public List<ConditionSO> conditions;

    public override bool IsMet()
    {
        if (conditions == null || conditions.Count == 0) return true;
        // 모든 조건이 null이 아니고, IsMet()이 true를 반환하는지 확인
        return conditions.All(condition => condition != null && condition.IsMet());
    }
}
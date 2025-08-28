using UnityEngine;

/// <summary>
/// 다음 Step으로 진행하기 위한 조건을 나타내는 모든 조건 ScriptableObject의 부모 클래스입니다.
/// </summary>
public abstract class ConditionSO : ScriptableObject
{
    /// <summary>
    /// 이 조건이 충족되었는지 여부를 반환합니다.
    /// </summary>
    /// <returns>조건 충족 시 true, 아니면 false</returns>
    public abstract bool IsMet();
}
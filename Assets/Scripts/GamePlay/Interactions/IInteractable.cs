using UnityEngine;

/// <summary>
/// 상호작용 가능한 모든 객체가 구현해야 하는 인터페이스입니다.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// 플레이어가 상호작용을 시도할 때 호출됩니다.
    /// </summary>
    /// <param name="caller">상호작용을 요청한 객체의 Transform</param>
    void Interact(Transform caller);

    /// <summary>
    /// 플레이어가 상호작용 범위에 들어오거나 나갈 때 아이콘 표시/숨김을 처리합니다.
    /// </summary>
    /// <param name="on">아이콘을 표시할지 여부</param>
    void ShowIcon(bool on);

    /// <summary>
    /// 게임 상태 변경에 따라 아이콘의 종류(대화 가능, 반복, 불가 등)를 갱신합니다.
    /// </summary>
    void UpdateIcon();
}
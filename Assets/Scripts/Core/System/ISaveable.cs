/// <summary>
/// 객체의 상태를 저장하고 복원하는 기능을 정의하는 인터페이스입니다.
/// 이 인터페이스를 구현하는 모든 컴포넌트는 GameSaveBridge에 의해 자동으로 관리됩니다.
/// </summary>
public interface ISaveable
{
    /// <summary>
    /// 이 객체를 식별하는 고유한 ID를 반환합니다. 씬 내에서 유일해야 합니다.
    /// </summary>
    string UniqueId { get; }

    /// <summary>
    /// 객체의 현재 상태를 직렬화 가능한 객체로 캡처하여 반환합니다.
    /// </summary>
    /// <returns>저장할 상태 데이터 객체.</returns>
    object CaptureState();

    /// <summary>
    /// 주어진 상태 데이터로 객체의 상태를 복원합니다.
    /// </summary>
    /// <param name="state">CaptureState에서 반환된 상태 데이터 객체.</param>
    void RestoreState(object state);
}
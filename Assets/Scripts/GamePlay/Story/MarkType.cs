/// <summary>
/// 게임 진행 상태를 기록하는 '마크(Mark)'의 종류를 정의합니다.
/// </summary>
public enum MarkType
{
    SYS,            // 시스템 내부 기록 (예: 스텝 진입 여부)
    NPC_TALKED,     // 특정 NPC와 대화 완료
    PROP_INSPECTED, // 특정 사물 조사 완료
    ROOM_VISITED,   // 특정 방 방문
    GATE_SUCCEEDED, // 특정 관문(Gate) 통과
}
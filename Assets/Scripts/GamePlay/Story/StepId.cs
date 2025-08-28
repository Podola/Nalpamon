/// <summary>
/// 스토리 스텝을 나타내는 열거형입니다.
/// StoryDabaseSO에 정의한 순서대로 진행됩니다.
/// </summary>
public enum StepId
{
    None = 0,

    /* Chapter 0 */
    C0_D0_00 = 1, // 프롤로그 시작
    C0_D0_01,

    /* Chapter 1 */
    C1_D1_00,
    C1_D1_01,
    C1_D1_02,
    C1_D1_03,
    C1_D1_04,
    C1_D1_05,
    C1_D1_06,
    C1_D1_07,
    C1_D1_08,
    C1_D1_09,
    C1_D1_10,

    // 새로운 챕터나 날짜가 추가될 때 여기에 이어서 정의
}
using UnityEngine;

/// <summary>
/// Chapter 1 씬의 시작과 흐름을 제어합니다.
/// </summary>
public class Chapter1Controller : ChapterControllerBase
{
    // --- ChapterControllerBase에 필요한 정보 제공 ---
    protected override int CurrentChapterNumber => 1;
    protected override int NextChapterNumber => 2;
    protected override string NextSceneName => "Chapter2Scene";

    [Header("챕터 진행 설정")]
    [Tooltip("이 스텝이 완료되면 Chapter 2로 넘어갑니다.")]
    [SerializeField] private StepId finalStepOfChapter1;
    protected override StepId FinalStepInChapter => finalStepOfChapter1;
    // ---------------------------------------------
}
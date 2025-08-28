using UnityEngine;

/// <summary>
/// Chapter 0 (프롤로그) 씬의 흐름을 제어합니다.
/// </summary>
public class Chapter0Controller : ChapterControllerBase
{
    // --- ChapterControllerBase에 필요한 정보 제공 ---
    protected override int CurrentChapterNumber => 0;
    protected override int NextChapterNumber => 1;
    protected override string NextSceneName => "Chapter1Scene";

    [Header("챕터 진행 설정")]
    [Tooltip("이 스텝이 완료되면 Chapter 1로 넘어갑니다.")]
    [SerializeField] private StepId finalStepOfChapter0;
    protected override StepId FinalStepInChapter => finalStepOfChapter0;
    // ---------------------------------------------

    #region Temp (테스트용 기능)
    private bool _isTransitioningForTest = false;

    /// <summary>
    /// 버튼 클릭 시 Chapter 1 씬을 시작합니다. (테스트용)
    /// </summary>
    public void StartChapter1()
    {
        if (_isTransitioningForTest) return;
        _isTransitioningForTest = true;

        Log.I("테스트: Chapter 1 시작 버튼 클릭됨. 데이터 준비 및 씬 전환을 시작합니다.");

        GameManager.Instance.SetupChapter(NextChapterNumber);
        // Chapter 1의 첫 스텝을 강제로 지정해줍니다.
        StepManager.Instance.EnterStep(StepId.C1_D1_00);

        SceneLoader.LoadScene(NextSceneName);
    }
    #endregion
}
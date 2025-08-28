using UnityEngine;

/// <summary>
/// 게임의 핵심 매니저들을 총괄하고 생명주기를 관리합니다.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    [Header("Chapter Asset Packages")]
    [Tooltip("챕터 순서대로 등록. 각 항목에 챕터 데이터와 스텝 데이터베이스를 함께 할당합니다.")]
    [SerializeField] public ChapterAssetPackage[] chapterPackages;

    private void Start()
    {
        // 게임 시작 시 필요한 초기화 로직이 있다면 여기에 작성할 수 있습니다.
        // 예를 들어, 각 매니저의 초기화 함수를 호출하는 순서를 제어할 수 있습니다.
        Log.I("GameManager와 모든 하위 매니저 초기화 완료.");
    }

    /// <summary>
    /// 지정된 챕터 번호에 맞는 데이터로 각 매니저를 설정합니다.
    /// 씬을 로드하기 직전에 호출됩니다.
    /// <param name="chapterNumber">시작할 챕터 번호 (1부터 시작)</param>
    /// </summary>
    public void SetupChapter(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= chapterPackages.Length)
        {
            Log.E($"유효하지 않은 챕터 인덱스({chapterIndex})입니다.");
            return;
        }

        Log.I($"--- 챕터 {chapterIndex} 준비 시작 ---");

        ChapterAssetPackage package = chapterPackages[chapterIndex];

        // 1. ChapterDataManager에 해당 챕터의 데이터를 주입하고 초기화
        ChapterDataManager.Instance.InitializeForChapter(package.chapterData);

        // 2. StepManager에 해당 챕터의 StepDatabase를 주입
        StepManager.Instance.SetupDatabase(package.stepDatabase);

        // 3. DialogueRunner에 이 챕터의 YarnProject를 설정합니다.
        if (DialogueManager.Instance != null && package.yarnProject != null)
        {
            DialogueManager.Instance.Runner.SetProject(package.yarnProject);
            Log.I($"YarnProject '{package.yarnProject.name}' 설정 완료.");
        }

        Log.I($"--- 챕터 {chapterIndex} 준비 완료 ---");
    }

    /// <summary>
    /// 지정된 챕터 번호에 해당하는 씬 파일의 이름을 반환합니다.
    /// </summary>
    public string GetSceneNameForChapter(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= chapterPackages.Length)
        {
            Log.E($"유효하지 않은 챕터 인덱스({chapterIndex})를 위한 씬 이름을 찾을 수 없습니다.");
            return null;
        }
        return chapterPackages[chapterIndex].sceneName;
    }

    /// <summary>
    /// '새 게임'을 시작하고 지정된 챕터의 초기 상태를 설정합니다.
    /// </summary>
    public void StartNewChapter(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= chapterPackages.Length) return;

        // 1. 일반 챕터 데이터 로드
        SetupChapter(chapterIndex);

        ChapterAssetPackage package = chapterPackages[chapterIndex];
        if (package.startState == null) return;

        // 2. StepManager 상태 완전 초기화
        StepManager.Instance.ClearAllMarks();
        foreach (string mark in package.startState.initialMarks)
        {
            StepManager.Instance.SetMark(mark, true, "NewGameInitial");
        }

        // 3. 첫 스텝으로 진입
        StepManager.Instance.EnterStep(package.startState.startStepId, true);

        // 4. 플레이어 시작 위치 정보를 임시 저장소에 기록
        SaveLaunchIntent.SetNewGameStartLocation(package.startState.startRoom, package.startState.startTargetPointKey);
    }
}
using UnityEngine;
using System.Collections;

/// <summary>
/// 모든 챕터 컨트롤러의 공통 로직을 담는 추상 기반 클래스입니다.
/// </summary>
public abstract class ChapterControllerBase : MonoBehaviour
{
    // 자식 클래스에서 반드시 자신의 정보를 제공하도록 강제합니다.
    protected abstract int CurrentChapterNumber { get; }
    protected abstract int NextChapterNumber { get; }
    protected abstract StepId FinalStepInChapter { get; }
    protected abstract string NextSceneName { get; }

    private bool _isTransitioning = false;

    private void OnEnable()
    {
        // StepManager의 스텝 변경 이벤트를 구독합니다.
        GameEvents.OnStepChanged += HandleStepChanged;
    }

    private void OnDisable()
    {
        // 메모리 누수를 방지하기 위해 구독을 해제합니다.
        GameEvents.OnStepChanged -= HandleStepChanged;
    }

    void Start()
    {
        Log.I($"Chapter {CurrentChapterNumber} 씬 시작됨.");
        // 여기에 각 챕터별 BGM 재생 등 공통 시작 로직을 추가할 수 있습니다.
    }

    /// <summary>
    /// 스텝이 변경될 때마다 호출되는 이벤트 핸들러입니다.
    /// </summary>
    private void HandleStepChanged(StepId previousStep, StepId currentStep)
    {
        // 이전 스텝이 이 챕터의 마지막 스텝이었는지 확인하고, 전환 중이 아닌지 체크합니다.
        if (previousStep == FinalStepInChapter && !_isTransitioning)
        {
            _isTransitioning = true;
            Log.I($"Chapter {CurrentChapterNumber} 마지막 스텝({FinalStepInChapter}) 완료. Chapter {NextChapterNumber}로 전환합니다.");

            // 다음 챕터로 전환하는 코루틴을 시작합니다.
            StartCoroutine(ProceedToNextChapter());
        }
    }

    private IEnumerator ProceedToNextChapter()
    {
        Log.I($"Chapter {CurrentChapterNumber} 완료. Chapter {NextChapterNumber} 데이터 설정 및 씬 전환을 시작합니다.");

        // '새 게임' 함수가 아닌, 다음 챕터 데이터를 '준비'하는 함수를 호출합니다.
        GameManager.Instance.SetupChapter(NextChapterNumber);

        // 이제 모든 초기화가 완료되었으므로 다음 챕터 씬을 로드합니다.
        SceneLoader.LoadScene(NextSceneName);
        yield break;
    }
}
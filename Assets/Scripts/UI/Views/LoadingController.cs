using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Loading 씬의 UI와 비동기 씬 로딩 과정을 제어합니다.
/// </summary>
public class LoadingController : MonoBehaviour
{
    [Header("옵션")]
    [Tooltip("로딩이 너무 빨라도 보장할 최소 로딩 시간(초)입니다.")]
    [SerializeField] private float minimumLoadingTime = 0.5f;

    [Header("디버그 옵션")]
    [Tooltip("true이면 로딩 완료 후 스페이스바를 눌러야 다음 씬으로 넘어갑니다.")]
    [SerializeField] private bool waitForManualActivation = true;

    [Header("UI")]
    [Tooltip("로딩 완료 시 활성화될 '계속하려면 스페이스바를 누르세요' UI 텍스트 오브젝트")]
    [SerializeField] private GameObject continueTextObject;

    private void Start()
    {
        StartCoroutine(LoadSceneProcess());
    }

    /// <summary>
    /// 실제 비동기 씬 로딩을 처리하는 코루틴입니다.
    /// </summary>
    private IEnumerator LoadSceneProcess()
    {
        if (continueTextObject != null)
        {
            continueTextObject.SetActive(false);
        }

        // 로딩 시작 시간 기록
        float startTime = Time.time;

        string sceneToLoad = SceneLoader.NextSceneToLoad;
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            yield return null;
        }

        // --- 로딩 완료 처리 ---

        // 현재까지 걸린 로딩 시간을 계산
        float elapsedTime = Time.time - startTime;

        // 만약 최소 로딩 시간보다 덜 걸렸다면, 남은 시간만큼 더 기다립니다.
        if (elapsedTime < minimumLoadingTime)
        {
            yield return new WaitForSeconds(minimumLoadingTime - elapsedTime);
        }

        // 디버그 옵션이 켜져 있다면, 수동 입력을 대기합니다.
        if (waitForManualActivation)
        {
            if (continueTextObject != null)
            {
                continueTextObject.SetActive(true);
            }
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }

        // 이제 다음 씬을 활성화합니다.
        op.allowSceneActivation = true;
    }
}
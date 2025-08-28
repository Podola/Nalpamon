using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// CoreScene에 배치되어, 모든 핵심 시스템이 초기화된 후 TitleScene으로 전환하는 역할을 합니다.
/// </summary>
public class CoreInitializer : MonoBehaviour
{
    void Start()
    {
        // DevStartHelper 대신 DebugManager의 설정을 확인합니다.
#if UNITY_EDITOR
        if (DebugManager.ShouldOverrideStart)
        {
            // 모든 재정의 로직은 이제 DebugManager가 담당합니다.
            var debugManager = DebugManager.Instance;
            int chapter = Util.GetChapterFromStepId(debugManager.targetStep);
            string sceneName = GameManager.Instance.GetSceneNameForChapter(chapter);

            if (!string.IsNullOrEmpty(sceneName))
            {
                debugManager.ApplyOverride();
                SceneLoader.LoadScene(sceneName);
            }
            else
            {
                Log.E($"해당 챕터({chapter})에 대한 씬 이름이 GameManager에 설정되지 않았습니다.");
            }
            return; // 재정의가 적용되었으므로 여기서 실행을 마칩니다.
        }
#endif

        // 재정의 설정이 없는 경우, 정상적으로 TitleScene으로 넘어갑니다.
        SceneManager.LoadScene("TitleScene");
    }
}
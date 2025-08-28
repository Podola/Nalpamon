using UnityEngine.SceneManagement;

/// <summary>
/// 씬 로드를 담당하는 정적 클래스입니다.
/// </summary>
public static class SceneLoader
{
    private static string nextScene;

    /// <summary>
    /// 지정된 씬을 로드합니다.
    /// </summary>
    /// <param name="sceneName">로드할 씬의 이름</param>
    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("LoadingScene"); // 항상 Loading 씬을 먼저 로드
    }

    // LoadSceneProcess 코루틴은 LoadingController로 이동합니다.
    // 대신, LoadingController가 다음에 로드할 씬 이름을 알 수 있도록 프로퍼티를 추가합니다.
    public static string NextSceneToLoad => nextScene;
}
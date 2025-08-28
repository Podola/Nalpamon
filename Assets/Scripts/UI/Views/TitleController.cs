using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 타이틀 씬의 UI와 흐름을 관리합니다.
/// TitleManager -> TitleController 클래스명 변경하였습니다.
/// </summary>
public class TitleController : MonoBehaviour
{
    [SerializeField] private Animator curtainAnimator;
    [SerializeField] private Animator panelAnimator;
    [SerializeField] private GameObject continueButton;

    private void Start()
    {
        // 게임 시작 시 설정 파일을 로드
        SaveLoadSystem.Load("Settings", "settings.json");

        // 자동 저장 파일이 있는지 확인하여 'Continue' 버튼 활성화 여부 결정
        if (continueButton != null)
        {
            continueButton.SetActive(SaveLoadSystem.Exists("autosave.json"));
        }

        // 타이틀 씬 애니메이션 시작
        StartCoroutine(TitleAnimation());
    }

    private System.Collections.IEnumerator TitleAnimation()
    {
        // 간단한 연출 코루틴
        curtainAnimator.Play("Open");
        yield return new WaitUntil(() => curtainAnimator.GetCurrentAnimatorStateInfo(0).IsName("Active"));
        panelAnimator.Play("Open");
    }

    /// <summary>
    /// 'Start' 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnClickStart()
    {
        Log.I("'새 게임'을 선택했습니다.");
        GameManager.Instance.StartNewChapter(0);
        SceneLoader.LoadScene("Chapter0Scene");
    }

    /// <summary>
    /// 'Continue' 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnClickContinue()
    {
        string autoSaveFile = "autosave.json";
        Log.I("'이어하기'를 선택했습니다.");

        if (SaveLoadSystem.Exists(autoSaveFile))
        {
            GameSaveBridge.Instance.LoadFromFile(autoSaveFile);
        }
        else
        {
            Log.W("자동 저장 파일을 찾을 수 없어 이어하기를 실행할 수 없습니다.");
        }
    }

    /// <summary>
    /// 'Settings' 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnClickSettings()
    {
        // TODO: 설정 UI를 활성화하는 로직 (SettingManager.Instance.SetActive(true);)
    }

    /// <summary>
    /// 'Exit' 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnClickExit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 실행 중지
#endif
    }
}
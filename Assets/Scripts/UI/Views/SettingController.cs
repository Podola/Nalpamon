using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정(Settings) UI 창을 총괄 관리하는 클래스입니다.
/// SettingManager -> SettingController로 이름 변경
/// </summary>
public class SettingController : MonoBehaviour
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button exitButton;

    [Header("Handles")]
    [SerializeField] private SettingScreenHandle screenHandle;
    [SerializeField] private SettingVolumeHandle volumeHandle;
    [SerializeField] private SettingLocaleHandle localeHandle;

    private void Start()
    {
        if (closeButton) closeButton.onClick.AddListener(() => SetActive(false));
        if (exitButton) exitButton.onClick.AddListener(Exit);

        // 각 설정 핸들러 초기화
        screenHandle?.Init();
        volumeHandle?.Init();
        localeHandle?.Init();
    }

    /// <summary>
    /// 설정 창을 활성화/비활성화합니다. 비활성화될 때 설정을 저장합니다.
    /// </summary>
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        InputManager.Instance?.SetPopupActive(active);

        if (!active)
        {
            SaveLoadSystem.Save("Settings", "settings.json");
            // SoundManager가 있다면 변경된 볼륨 즉시 적용
            SoundManager.Instance?.LoadAndApplyVolumeSettings();
        }
    }

    private void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
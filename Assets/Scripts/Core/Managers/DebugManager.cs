// 파일 경로: Scripts/1_Core/Managers/DebugManager.cs

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum LogLevel
{
    None,
    ErrorsOnly,
    Normal,
    Verbose
}

/// <summary>
/// 게임의 모든 개발/디버그 관련 설정을 총괄하는 중앙 관리자입니다.
/// </summary>
public class DebugManager : Singleton<DebugManager>
{
    [Header("글로벌 개발자 모드")]
    [Tooltip("True이면 모든 개발 기능이 활성화됩니다.")]
    public bool IsDevMode = true;

    [Header("로그 레벨 설정")]
    public LogLevel LogLevel = LogLevel.Normal;

    [Header("인게임 디버그 UI")]
    [Tooltip("인게임 UI에 표시할 최대 Mark 개수")]
    public int maxMarksInUI = 15;

    [Header("개발용 시작 지점 재정의 (에디터 전용)")]
    [Tooltip("Developer Console 창을 통해 제어됩니다.")]
    public bool overrideStartPoint = false;
    public StepId targetStep = StepId.None;
    public RoomType targetRoom = RoomType.None;
    public string targetPointKey = "Default";

    public List<string> AllTrackedMarks { get; } = new List<string>();
    public List<string> RecentTrackedMarks { get; } = new List<string>();

    public static bool ShouldOverrideStart => Instance != null && Instance.overrideStartPoint && Instance.IsDevMode;

    protected override void Awake()
    {
        base.Awake();
#if !UNITY_EDITOR
        IsDevMode = false;
        gameObject.SetActive(false);
#endif
    }

    private void OnEnable()
    {
        GameEvents.OnMarkChanged += HandleMarkChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnMarkChanged -= HandleMarkChanged;
    }

    private void HandleMarkChanged(string key, bool value)
    {
        if (!IsDevMode) return;

        // --- 전체 Mark 목록 관리 (For Developer Console) ---
        AllTrackedMarks.Remove(key);
        if (value)
        {
            AllTrackedMarks.Add(key);
            AllTrackedMarks.Sort();
        }

        // --- 최근 Mark 목록 관리 (For In-Game DebugDisplay) ---
        RecentTrackedMarks.Remove(key);
        if (value)
        {
            // 가장 최근에 활성화된 Mark가 목록의 맨 앞에 오도록 추가
            RecentTrackedMarks.Insert(0, key);
        }

        // 최대 개수 초과 시 가장 오래된 (목록의 맨 뒤) Mark 제거
        while (RecentTrackedMarks.Count > maxMarksInUI)
        {
            RecentTrackedMarks.RemoveAt(RecentTrackedMarks.Count - 1);
        }
    }

    public void ApplyOverride()
    {
        if (!overrideStartPoint) return;
        Log.W($"[개발용 헬퍼] 시작 지점 재정의: Step({targetStep}), Room({targetRoom}:{targetPointKey})");
        int chapter = Util.GetChapterFromStepId(targetStep);
        GameManager.Instance.SetupChapter(chapter);
        StepManager.Instance.EnterStep(targetStep, true);
        SaveLaunchIntent.SetNewGameStartLocation(targetRoom, targetPointKey);
    }
}
// 파일 경로: Scripts/1_Core/Managers/StepManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables; // 타임라인 재생을 위해 추가
using Yarn.Unity;
using static Util;

/// <summary>
/// 게임의 스토리 진행 단계(Step)와 상태(Mark)를 중앙에서 관리합니다.
/// GameManager에 의해 StepDatabaseSO가 주입된 후 활성화됩니다.
/// </summary>
public class StepManager : Singleton<StepManager>, ISaveable
{
    /// <summary>
    /// NpcPlacementSystem에서 사용할 배치 타이밍 정의
    /// </summary>
    public enum ApplyWhen
    {
        OnEnter,             // 스텝 진입 시 즉시 적용
        AfterEntryDialogue,  // 스텝 진입 대사가 끝난 직후 적용
        OnExitPrev           // 이전 스텝에서 나갈 때 적용
    }

    // 데이터베이스는 이제 GameManager가 외부에서 주입합니다.
    public StepDatabaseSO Database { get; private set; }
    public StepId CurrentStep { get; private set; } = StepId.None;
    public string CurrentTimeOfDay { get; private set; } = "Day";

    private readonly Dictionary<string, bool> _marks = new();
    private bool _isInitialized = false;
    private bool _progressCheckScheduled;

#if UNITY_EDITOR
    public IReadOnlyDictionary<string, bool> MarksSnapshot => _marks;
#endif

    private void Start()
    {
        // DialogueRunner와의 연동은 미리 해두어도 안전합니다.
        var runner = FindFirstObjectByType<DialogueRunner>();
        if (runner != null)
        {
            runner.AddFunction("GetStoryStep", () => (int)CurrentStep);
            runner.AddFunction("CheckMark", (string key) => GetMark(key));
            runner.AddCommandHandler("AddMark", (string key, bool value) => SetMark(key, value, "Yarn"));
            runner.AddCommandHandler("SetTimeOfDay", (string time) => SetTimeOfDay(time));
        }
    }

    public void SetTimeOfDay(string time)
    {
        if (CurrentTimeOfDay == time) return;

        CurrentTimeOfDay = time;
        Log.I($"시간대가 '{time}'(으)로 변경되었습니다.");

        // 시간대 변경 이벤트를 게임 전체 발송.
        GameEvents.TriggerTimeOfDayChanged(CurrentTimeOfDay);
    }

    /// <summary>
    /// GameManager가 챕터 시작 시 호출하는 초기화 메서드입니다.
    /// </summary>
    public void SetupDatabase(StepDatabaseSO database)
    {
        if (database == null)
        {
            Log.E("StepDatabaseSO가 null입니다. 초기화에 실패했습니다.");
            _isInitialized = false;
            return;
        }
        this.Database = database;
        Log.I($"'{database.name}' 데이터베이스 로드 완료.");

        _isInitialized = true;
    }

    /// <summary>
    /// 지정된 스텝의 설정 정보를 가져옵니다. 데이터베이스가 없으면 null을 반환합니다.
    /// </summary>
    public StepDatabaseSO.StepConfig GetConfig(StepId step)
    {
        if (!_isInitialized || Database == null) return null;
        return Database.steps.FirstOrDefault(s => s.step == step);
    }

    /// <summary>
    /// 지정된 키의 마크 상태를 반환합니다.
    /// </summary>
    public bool GetMark(string key) => _marks.TryGetValue(key, out var v) && v;

    /// <summary>
    /// 마크 상태를 설정하고 변경 시 이벤트를 발생시킵니다.
    /// </summary>
    public void SetMark(string key, bool value, string reason = null)
    {
        bool changed = !_marks.TryGetValue(key, out var prev) || prev != value;
        _marks[key] = value;
        if (!changed) return;

        if (DebugManager.Instance != null && DebugManager.Instance.LogLevel >= LogLevel.Verbose)
        {
            string reasonText = string.IsNullOrEmpty(reason) ? "" : $" (원인: {reason})";
            Log.V($"[MARK] {key} -> {value}{reasonText}");
        }

        GameEvents.TriggerMarkChanged(key, value);

        if (_isInitialized)
        {
            TryProgress();
        }
    }

    /// <summary>
    /// 새로운 스텝으로 진입합니다.
    /// </summary>
    public void EnterStep(StepId next, bool firstEnter = false)
    {
        var prev = CurrentStep;
        CurrentStep = next;

        Log.I($"단계 변경: {prev} -> {next}");
        GameEvents.TriggerStepChanged(prev, next);

        var config = GetConfig(next);
        if (config != null)
        {
            // 새로운 enum 값을 기준으로 분기 처리
            switch (config.entryType)
            {
                case StepEntryType.Timeline:
                    PlayTimeline(config.entryTimeline);
                    break;
                case StepEntryType.Dialogue:
                    PlayStepEntryIfAny(next);
                    break;
                case StepEntryType.None:
                    // 아무것도 하지 않음
                    break;
            }
        }

        if (!firstEnter) TryProgress();

        if (GameSaveBridge.Instance != null)
        {
            GameSaveBridge.Instance.SaveAutoStep();
        }
    }

    private void PlayTimeline(PlayableAsset timelineAsset)
    {
        var director = FindFirstObjectByType<PlayableDirector>();
        if (director != null)
        {
            director.playableAsset = timelineAsset;
            director.Play();
            Log.I($"'{timelineAsset.name}' 타임라인을 재생합니다.");
        }
        else
        {
            Log.W("씬에서 PlayableDirector를 찾을 수 없어 타임라인을 재생할 수 없습니다.");
        }
    }

    private void PlayStepEntryIfAny(StepId step)
    {
        string playedKey = StepEntryKey(step);
        if (GetMark(playedKey)) return;

        string node = StepEntryNode(step);
        if (!NodeExists(node, false)) return;
        if (DialogueManager.Instance == null || DialogueManager.Instance.IsRunning) return;

        var stepAtStart = step;

        UnityEngine.Events.UnityAction onComplete = null;
        onComplete = () =>
        {
            DialogueManager.Instance.Runner.onDialogueComplete.RemoveListener(onComplete);
            SetMark(StepEntryKey(stepAtStart), true, "StepEntry");
        };
        DialogueManager.Instance.Runner.onDialogueComplete.AddListener(onComplete);
        DialogueManager.StartDialogue(node);
    }

    // ... 나머지 CanInteractNow, TryCompleteTask, ISaveable 구현 등 모든 기존 코드는 그대로 유지 ...
    public bool GetDaily(MarkType type, string name) => GetMark(MarkDaily(type, name));
    public void SetDaily(MarkType type, string name, bool value, string reason = null)
        => SetMark(MarkDaily(type, name), value, reason);
    public bool CanInteractNow(MarkType type, string name, out StepDatabaseSO.InteractTask expected, out bool eligible)
    {
        expected = FindPendingTask(type, name);
        eligible = false;

        var cfg = GetConfig(CurrentStep);
        if (cfg == null) return true;

        bool isStory = cfg.stepMode == StepDatabaseSO.StepMode.Story;
        bool enforce = isStory || cfg.enforceOrder;

        int? minPending = GetMinPendingOrder();

        if (isStory)
        {
            if (expected == null) return false;
            if (enforce && minPending.HasValue && expected.order != minPending.Value) return false;

            eligible = true;
            return true;
        }
        else
        {
            if (expected == null)
            {
                eligible = false;
                return true;
            }

            if (enforce && minPending.HasValue && expected.order != minPending.Value)
            {
                eligible = false;
                return true;
            }

            eligible = true;
            return true;
        }
    }
    public bool TryCompleteTask(MarkType type, string name, string reason = null)
    {
        if (!CanInteractNow(type, name, out var task, out var eligible)) return false;
        if (task == null || !eligible) return false;

        string key = GetTaskDoneKey(task, type, name);
        Log.I($"과업 완료: '{name}' ({type})");
        SetMark(key, true, reason ?? $"{type}/{name}");
        return true;
    }
    private int? GetMinPendingOrder()
    {
        var cfg = GetConfig(CurrentStep);
        if (cfg == null || cfg.tasks == null || cfg.tasks.Count == 0) return null;

        int? min = null;
        foreach (var t in cfg.tasks)
        {
            string k = GetTaskDoneKey(t, t.type, t.name);
            if (GetMark(k)) continue;

            if (!min.HasValue || t.order < min.Value) min = t.order;
        }
        return min;
    }
    public StepDatabaseSO.InteractTask FindPendingTask(MarkType type, string name)
    {
        var cfg = GetConfig(CurrentStep);
        if (cfg == null || cfg.tasks == null) return null;

        return cfg.tasks
            .Where(t => t.type == type && t.name == name)
            .OrderBy(t => t.order)
            .FirstOrDefault(t => !GetMark(GetTaskDoneKey(t, type, name)));
    }
    private bool AreAllRequiredTasksDone()
    {
        var cfg = GetConfig(CurrentStep);
        if (cfg == null || cfg.tasks == null) return true;
        return cfg.tasks.All(t => GetMark(GetTaskDoneKey(t, t.type, t.name)));
    }
    private bool IsProgressRequirementMet()
    {
        var cfg = GetConfig(CurrentStep);
        if (cfg == null || cfg.progressConditions == null || cfg.progressConditions.Count == 0) return true;
        return cfg.progressConditions.All(condition => condition != null && condition.IsMet());
    }
    private void TryProgress()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsRunning)
        {
            if (!_progressCheckScheduled)
            {
                _progressCheckScheduled = true;
                StartCoroutine(WaitAndTryProgress());
            }
            return;
        }

        if (!AreAllRequiredTasksDone() || !IsProgressRequirementMet()) return;

        int idx = Database.steps.FindIndex(s => s.step == CurrentStep);
        if (idx < 0 || idx + 1 >= Database.steps.Count) return;

        EnterStep(Database.steps[idx + 1].step);
    }
    private System.Collections.IEnumerator WaitAndTryProgress()
    {
        while (DialogueManager.Instance != null && DialogueManager.Instance.IsRunning)
            yield return null;
        _progressCheckScheduled = false;
        TryProgress();
    }
    public string GetTaskDoneKey(StepDatabaseSO.InteractTask task, MarkType type, string name)
    {
        var cfg = GetConfig(CurrentStep);
        bool hasDuplicate = cfg?.tasks.Count(t => t.type == type && t.name == name) > 1;
        string uniqueName = hasDuplicate ? $"{name}#{task.order}" : name;
        return Mark(type, uniqueName, CurrentStep);
    }
    public void ForceRefreshAll()
    {
        foreach (var kv in _marks)
            GameEvents.TriggerMarkChanged(kv.Key, kv.Value);

        GameEvents.TriggerStepChanged(CurrentStep, CurrentStep);
    }
    public string UniqueId => "StepManager";
    [Serializable]
    public class SaveDataV1
    {
        public StepId currentStep;
        public List<string> marksTrue = new List<string>();
        public string currentTimeOfDay;
    }
    public object CaptureState()
    {
        return new SaveDataV1
        {
            currentStep = this.CurrentStep,
            marksTrue = _marks.Where(kv => kv.Value).Select(kv => kv.Key).ToList(),
            currentTimeOfDay = this.CurrentTimeOfDay
        };
    }
    public void RestoreState(object state)
    {
        if (state is SaveDataV1 data)
        {
            _marks.Clear();
            if (data.marksTrue != null)
            {
                foreach (var k in data.marksTrue) _marks[k] = true;
            }
            CurrentStep = data.currentStep;
            CurrentTimeOfDay = data.currentTimeOfDay;
            Log.I($"상태 복원 완료. 현재 단계: {CurrentStep}, 현재 시간: {CurrentTimeOfDay}");
        }
    }
    public void ClearAllMarks()
    {
        _marks.Clear();
        Log.I("'새 게임' 시작. 모든 Mark 기록을 초기화합니다.");
    }
}
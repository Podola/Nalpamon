using System;
using UnityEngine;

/// <summary>
/// 게임의 주요 이벤트를 관리하는 중앙 허브입니다.
/// 매니저 간의 직접적인 종속성을 줄이고 느슨한 결합을 유지합니다.
/// </summary>
public static class GameEvents
{
    // StepManager Events
    public static event Action<StepId, StepId> OnStepChanged;
    public static void TriggerStepChanged(StepId previous, StepId current) => OnStepChanged?.Invoke(previous, current);

    public static event Action<string, bool> OnMarkChanged;
    public static void TriggerMarkChanged(string key, bool value) => OnMarkChanged?.Invoke(key, value);

    // DialogueManager Events
    public static event Action OnDialogueStarted;
    public static void TriggerDialogueStarted() => OnDialogueStarted?.Invoke();

    public static event Action OnDialogueCompleted;
    public static void TriggerDialogueCompleted() => OnDialogueCompleted?.Invoke();

    // RoomManager Events
    public static event Action<RoomType, RoomType> OnRoomChanged;
    public static void TriggerRoomChanged(RoomType previous, RoomType current) => OnRoomChanged?.Invoke(previous, current);

    // TimeOfDay Events
    public static event Action<string> OnTimeOfDayChanged;
    public static void TriggerTimeOfDayChanged(string currentTime) => OnTimeOfDayChanged?.Invoke(currentTime);
}
// 파일 경로: Scripts/1_Core/Data/SO/StepDatabaseSO.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables; // Timeline 에셋을 위해 추가

/// <summary>
/// 스텝 진입 시 실행될 연출의 종류를 정의합니다.
/// </summary>
public enum StepEntryType
{
    None,      // 아무 연출 없음
    Dialogue,  // 간단한 진입 대사 재생
    Timeline   // 시네마틱 타임라인 재생
}

/// <summary>
/// 모든 배치는 Step 진입 시점에 즉시 이루어집니다.
/// </summary>
[Serializable]
public class NpcPlacementConfig
{
    [Tooltip("배치할 NPC의 ID")]
    public NpcId npcId;

    [Tooltip("배치될 방")]
    public RoomType room;

    [Tooltip("해당 방에 있는 TargetPoint의 고유 Key")]
    public string targetPointKey;
}

/// <summary>
/// 게임의 모든 스토리 단계(Step)와 그에 따른 과업(Task), 진행 조건들을 정의하는 데이터 에셋입니다.
/// </summary>
[CreateAssetMenu(fileName = "StepDatabase", menuName = "Game/StepDatabase")]
public class StepDatabaseSO : ScriptableObject
{
    /// <summary>
    /// 스텝의 진행 모드를 정의합니다.
    /// </summary>
    public enum StepMode
    {
        Story,  // 순서가 강제되며, 필수 과업 외 상호작용이 제한될 수 있음
        Free    // 자유로운 상호작용 가능, 순서는 enforceOrder에 따름
    }

    /// <summary>
    /// 각 스텝에서 수행해야 할 상호작용 과업(Task)을 정의합니다.
    /// </summary>
    [Serializable]
    public class InteractTask
    {
        public MarkType type = MarkType.NPC_TALKED;
        public string name;
        public int order = 0;
        public string nodeBaseOverride; // 대화 노드 이름 재정의
    }

    /// <summary>
    /// 한 스텝의 모든 설정을 담는 클래스입니다.
    /// </summary>
    [Serializable]
    public class StepConfig
    {
        public StepId step = StepId.None;

        [Header("Entry Event")]
        [Tooltip("스텝 진입 시 실행될 연출의 종류를 선택하세요.")]
        public StepEntryType entryType = StepEntryType.Dialogue;

        [Tooltip("Entry Type이 'Timeline'일 경우에만 사용됩니다.")]
        public PlayableAsset entryTimeline;

        [Tooltip("스텝의 진행 모드 (Story/Free)")]
        public StepMode stepMode = StepMode.Story;
        [Tooltip("Free 모드일 때도 Task의 order 순서를 강제할지 여부")]
        public bool enforceOrder = false;
        [TextArea(3, 5)]
        public string note; // 기획용 메모

        [Header("NPC Placements")]
        [Tooltip("이 스텝에서 적용될 모든 NPC의 위치/상태 목록")]
        public List<NpcPlacementConfig> npcPlacements = new List<NpcPlacementConfig>();

        [Header("Tasks")]
        [Tooltip("이 스텝에서 완료해야 할 과업 목록")]
        public List<InteractTask> tasks = new List<InteractTask>();

        [Header("Extra progress conditions")]
        [Tooltip("모든 Task 완료 후, 다음 스텝으로 넘어가기 위한 추가 조건 목록")]
        public List<ConditionSO> progressConditions = new List<ConditionSO>();

        [Header("Optional Dialogues")]
        [Tooltip("이 스텝에 StepGate가 존재하며, 관련 노드를 자동 관리합니다.")]
        public bool useStepGate;

        [Tooltip("Task와 무관하게 이 스텝에서 사용될 일반 NPC 대화 목록 (NPC 오브젝트 이름)")]
        public List<string> generalNpcDialogues = new List<string>();

        [Tooltip("이 스텝에서 처음 진입 시 대사를 재생할 방 목록")]
        public List<RoomType> roomEntryDialogues = new List<RoomType>();
    }

    // 데이터베이스에 포함된 모든 스텝 목록
    public List<StepConfig> steps = new List<StepConfig>();

    [Tooltip("반복 대사가 없을 경우 사용할 기본 폴백 대화 노드 이름")]
    public string fallbackRepeatNode = "Repeat_Default";
}
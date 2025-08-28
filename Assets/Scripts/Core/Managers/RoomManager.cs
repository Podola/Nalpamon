using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Util;
using Unity.Cinemachine;

public sealed class RoomManager : Singleton<RoomManager>
{
    [Header("카메라 제어")]
    [Tooltip("씬에 있는 RoomStateCamera의 Animator")]
    public Animator roomCameraAnimator;

    // Room과 VCam을 각각의 딕셔너리에서 관리합니다.
    private readonly Dictionary<RoomType, RoomController> _rooms = new();
    private readonly Dictionary<RoomType, CinemachineCamera> _vcams = new();

    private RoomController _current;
    private PlayerController _playerController;

    public bool playRoomEntryDialogue = true;
    public RoomType CurrentRoomType => _current ? _current.Type : RoomType.None;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _playerController = FindFirstObjectByType<PlayerController>();

        var stateDrivenCamera = FindFirstObjectByType<CinemachineStateDrivenCamera>();
        if (stateDrivenCamera != null)
        {
            roomCameraAnimator = stateDrivenCamera.GetComponent<Animator>();
        }

        CollectRoomsAndVCamsInCurrentScene(); // Room과 VCam을 함께 수집

        if (_rooms.Count > 0)
        {
            StartCoroutine(InitializeStartupRoom());
        }
    }

    public void SwitchRoom(RoomType to, bool playEntryThisTime = true)
    {
        if (!_rooms.TryGetValue(to, out var next))
        {
            Log.E($"전환할 방({to})을 찾을 수 없습니다.");
            return;
        }
        if (_current == next) return;

        var prevType = _current ? _current.Type : RoomType.None;
        _current = next;

        // VCam 타겟 설정
        if (_vcams.TryGetValue(to, out var targetVCam) && targetVCam != null)
        {
            if (_playerController != null)
            {
                targetVCam.Follow = _playerController.transform;
                targetVCam.LookAt = _playerController.transform;
            }
            else
            {
                Log.W($"PlayerController를 찾을 수 없어 {to} VCam의 타겟을 설정할 수 없습니다.");
            }
        }

        if (roomCameraAnimator != null)
        {
            roomCameraAnimator.Play(to.ToString());
            Log.V($"Room Camera Animator 상태를 '{to.ToString()}'(으)로 변경.");
        }
        else
        {
            Log.W("RoomManager에 roomCameraAnimator가 연결되지 않았습니다.");
        }

        Log.I($"방 변경: {prevType} -> {_current.Type}");
        GameEvents.TriggerRoomChanged(prevType, _current.Type);

        if (playRoomEntryDialogue && playEntryThisTime)
            TryPlayRoomEntryDialogue(to);
    }

    private void CollectRoomsAndVCamsInCurrentScene()
    {
        _rooms.Clear();
        _vcams.Clear();
        _current = null;

        // Room 수집
        var foundRooms = FindObjectsByType<RoomController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var rc in foundRooms)
        {
            if (rc.Type != RoomType.None && !_rooms.ContainsKey(rc.Type))
            {
                _rooms.Add(rc.Type, rc);
            }
        }
        Log.V($"현재 씬에서 {foundRooms.Length}개의 방을 찾았습니다.");

        // VCam 수집 (StateDrivenCamera 하위에서 이름 규칙으로 찾기)
        if (roomCameraAnimator != null)
        {
            var allVCams = roomCameraAnimator.GetComponentsInChildren<CinemachineCamera>(true);
            foreach (var vcam in allVCams)
            {
                // 이름 규칙: VCam_RoomType
                string expectedRoomName = vcam.name.Replace("VCam_", "");
                if (System.Enum.TryParse<RoomType>(expectedRoomName, out var roomType))
                {
                    if (!_vcams.ContainsKey(roomType))
                    {
                        _vcams.Add(roomType, vcam);
                    }
                }
            }
            Log.V($"현재 씬에서 {_vcams.Count}개의 VCam을 찾았습니다.");
        }
    }

    private IEnumerator InitializeStartupRoom()
    {
        yield return new WaitForEndOfFrame();
        if (SaveLaunchIntent.IsNewGame)
        {
            RoomType startRoom = SaveLaunchIntent.NewGameRoom;
            if (_rooms.ContainsKey(startRoom)) SwitchRoom(startRoom, false);
            else Log.E($"'새 게임' 시작 방({startRoom})을 현재 씬에서 찾을 수 없습니다.");
        }
        else
        {
            if (_playerController != null)
            {
                var detectedRoom = DetectRoomByPosition(_playerController.transform.position);
                if (detectedRoom != RoomType.None) SwitchRoom(detectedRoom, false);
                else
                {
                    Log.W("저장된 플레이어 위치에서 방을 찾지 못했습니다. 첫 번째 방에서 시작합니다.");
                    if (_rooms.Count > 0) SwitchRoom(_rooms.Keys.First(), false);
                }
            }
        }
    }
    private void TryPlayRoomEntryDialogue(RoomType room)
    {
        var dm = DialogueManager.Instance;
        if (dm == null || dm.IsRunning) return;
        var stepManager = StepManager.Instance;
        if (stepManager == null || stepManager.CurrentStep == StepId.None) return;
        string stepNode = RoomEntryNode(room, stepManager.CurrentStep);
        if (!NodeExists(stepNode, false)) return;
        UnityEngine.Events.UnityAction onComplete = null;
        onComplete = () =>
        {
            dm.Runner.onDialogueComplete.RemoveListener(onComplete);
            stepManager.SetMark(RoomVisitedInStep(room, stepManager.CurrentStep), true, "RoomEntryComplete");
        };
        dm.Runner.onDialogueComplete.AddListener(onComplete);
        DialogueManager.StartDialogue(stepNode);
    }
    private RoomType DetectRoomByPosition(Vector3 worldPos)
    {
        foreach (var kv in _rooms)
        {
            if (kv.Value != null && kv.Value.Contains(worldPos))
                return kv.Key;
        }
        return RoomType.None;
    }
}
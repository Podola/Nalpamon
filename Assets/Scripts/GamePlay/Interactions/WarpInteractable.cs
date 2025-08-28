// 파일 경로: Scripts/2_GamePlay/World/Interaction/WarpInteractable.cs
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 다른 방(Room)으로 플레이어를 이동시키는 워프 트리거입니다.
/// TargetPoint 시스템을 사용하여 목적지를 지정합니다.
/// </summary>
public sealed class WarpInteractable : MonoBehaviour, IInteractable
{
    /// <summary>
    /// 플레이어가 워프를 통해 이동을 완료했을 때 호출되는 이벤트입니다.
    /// 파라미터: (플레이어의 Transform, 도착한 TargetPoint의 Transform)
    /// </summary>
    public static event Action<Transform, Transform> OnPlayerWarped;

    [Header("Warp Settings")]
    public WarpType Type = WarpType.Door;
    [Tooltip("이동할 목표 방(Room)")]
    public RoomType targetRoom;
    [Tooltip("목표 방에 있는 TargetPoint의 Key")]
    public string spawnPointKey;

    [Header("Icon")]
    public GameObject IconRoot;
    public GameObject DoorIcon;
    public GameObject MoveIcon;
    public GameObject MoveLeftIcon;
    public GameObject MoveRightIcon;
    public GameObject StairsIcon;

    [Header("Restrictions")]
    [Tooltip("Story 모드에서 기본적으로 이동을 금지할지 여부")]
    public bool restrictInStory = true;
    [Tooltip("Story 모드 중에도 이동을 허용할 특정 스텝 목록")]
    public StepId[] allowInStorySteps;

    [Header("Facing Override")]
    [Tooltip("이동 후 플레이어의 방향을 강제로 지정합니다.")]
    public bool overrideFacing = false;
    [Tooltip("true면 오른쪽, false면 왼쪽을 바라보게 합니다.")]
    public bool overrideFaceRight = true;

    private static bool _isWarping; // 중복 워프 방지

#if UNITY_EDITOR
    // 에디터의 편의성을 위해 추가하는 임시 필드
    [SerializeField, HideInInspector]
    private TargetPoint _editor_DestinationLink;
#endif

    private void OnEnable()
    {
        ShowIcon(false);
    }

    private void OnValidate()
    {
        if (DoorIcon) DoorIcon.SetActive(Type == WarpType.Door);
        if (MoveIcon) MoveIcon.SetActive(Type == WarpType.Move);
        if (MoveRightIcon) MoveRightIcon.SetActive(Type == WarpType.MoveRight);
        if (MoveLeftIcon) MoveLeftIcon.SetActive(Type == WarpType.MoveLeft);
        if (StairsIcon) StairsIcon.SetActive(Type == WarpType.Stairs);
    }

    public void ShowIcon(bool on)
    {
        if (IconRoot == null) return;
        if (InputManager.Instance != null && InputManager.Instance.IsGloballyLocked)
        {
            IconRoot.SetActive(false);
            return;
        }
        IconRoot.SetActive(on && CanWarpNow());
    }

    public void UpdateIcon()
    {
        if (IconRoot != null && IconRoot.activeInHierarchy)
        {
            IconRoot.SetActive(CanWarpNow());
        }
    }

    public void Interact(Transform caller)
    {
        if (_isWarping || !CanWarpNow() || (DialogueManager.Instance != null && DialogueManager.Instance.IsRunning))
        {
            return;
        }
        StartCoroutine(WarpRoutine(caller));
    }

    private bool CanWarpNow()
    {
        if (targetRoom == RoomType.None || string.IsNullOrEmpty(spawnPointKey)) return false;
        var sm = StepManager.Instance;
        if (sm == null) return true;
        var cfg = sm.GetConfig(sm.CurrentStep);
        bool isStory = cfg != null && cfg.stepMode == StepDatabaseSO.StepMode.Story;
        if (!isStory || !restrictInStory) return true;
        if (allowInStorySteps != null)
        {
            for (int i = 0; i < allowInStorySteps.Length; i++)
                if (allowInStorySteps[i] == sm.CurrentStep) return true;
        }
        return false;
    }

    private IEnumerator WarpRoutine(Transform caller)
    {
        _isWarping = true;
        InputManager.Instance.IsGloballyLocked = true;

        try
        {
            Transform dest = TargetPointRegistry.Get(targetRoom, spawnPointKey);
            if (dest == null)
            {
                Log.E($"Warp 실패: TargetPoint를 찾을 수 없습니다. Room: '{targetRoom}', Key: '{spawnPointKey}'");
                yield break;
            }

            var visual = caller.GetComponent<PlayerController>()?.VisualRoot ?? caller;
            bool enteredFacingRight = visual.localScale.x >= 0f;

            yield return ScreenFader.FadeOutRoutine(0.2f);

            caller.position = dest.position;
            yield return new WaitForFixedUpdate();
            RoomManager.Instance.SwitchRoom(targetRoom);

            // NpcPlacementSystem에 플레이어 이동 완료를 알립니다.
            OnPlayerWarped?.Invoke(caller, dest);

            bool finalRight = DecideFinalFacing(enteredFacingRight, dest);
            var scale = visual.localScale;
            scale.x = Mathf.Abs(scale.x) * (finalRight ? 1 : -1);
            visual.localScale = scale;

            yield return new WaitForSeconds(0.05f);
            yield return ScreenFader.FadeInRoutine(0.2f);
        }
        finally
        {
            InputManager.Instance.IsGloballyLocked = false;
            _isWarping = false;
        }
    }

    private bool DecideFinalFacing(bool enteredFacingRight, Transform destination)
    {
        if (overrideFacing) return overrideFaceRight;
        if (destination.localScale.x != 0) return destination.localScale.x > 0;
        return Type switch
        {
            WarpType.MoveLeft => false,
            WarpType.MoveRight => true,
            _ => enteredFacingRight,
        };
    }
}
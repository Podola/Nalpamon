using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;
using Yarn.Markup;
using TMPro;
using System.Threading;

/// <summary>
/// 플레이어의 모든 입력을 관리하고, 게임의 상태(대화, UI, 플레이)에 따라 입력을 제어합니다.
/// </summary>
public class InputManager : Singleton<InputManager>, IActionMarkupHandler
{
    /// <summary>
    /// 현재 게임의 입력 상태를 정의합니다.
    /// </summary>
    public enum EInputState
    {
        PlayerControl,  // 플레이어 조작 가능
        Dialogue,       // 대화 진행 중
        Popup,          // 노트, 설정 등 UI 팝업 활성화
        GloballyLocked  // 씬 전환, 컷신 등 모든 입력 잠금
    }

    /// <summary>
    /// 현재 입력 상태를 반환합니다. GameStatusDebugger에서 이 값을 읽어갑니다.
    /// </summary>
    public EInputState CurrentState
    {
        get
        {
            if (IsGloballyLocked) return EInputState.GloballyLocked;
            if (isDialogueActive) return EInputState.Dialogue;
            if (isPopupActive) return EInputState.Popup;
            return EInputState.PlayerControl;
        }
    }

    private PlayerController _player;
    private DialogueRunner _dialogueRunner;

    private bool isDialogueActive = false;
    private bool isPopupActive = false;
    private bool _isLineDisplaying = false;

    public bool IsGloballyLocked { get; set; }

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnEnable()
    {
        GameEvents.OnDialogueStarted += HandleDialogueStart;
        GameEvents.OnDialogueCompleted += HandleDialogueComplete;
    }

    private void OnDisable()
    {
        GameEvents.OnDialogueStarted -= HandleDialogueStart;
        GameEvents.OnDialogueCompleted -= HandleDialogueComplete;
    }

    private void Start()
    {
        _dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        FindPlayerController();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayerController();
    }

    private void FindPlayerController() => _player = FindFirstObjectByType<PlayerController>();

    private void Update()
    {
        // 현재 상태에 따라 입력을 분기하여 처리합니다.
        switch (CurrentState)
        {
            case EInputState.PlayerControl:
                if (_player != null) HandlePlayerInput();
                break;
            case EInputState.Dialogue:
                HandleDialogueInput();
                break;
            case EInputState.Popup:
                // 이곳에 나중에 ESC 키로 UI를 닫는 등의 공통 팝업 입력 로직을 추가할 수 있습니다.
                break;
            case EInputState.GloballyLocked:
                // 모든 입력을 무시합니다.
                break;
        }
    }

    private void HandlePlayerInput()
    {
        float move = Input.GetAxisRaw("Horizontal");
        _player.Move(move);

        bool runHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        _player.SetRunning(runHeld);

        if (Input.GetKeyDown(KeyCode.W)) _player.Jump();
        if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Space)) _player.Interact();
    }

    private void HandleDialogueInput()
    {
        if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Space))
        {
            if (_isLineDisplaying)
            {
                _dialogueRunner.RequestHurryUpLine();
            }
            else
            {
                _dialogueRunner.RequestNextLine();
            }
        }
    }

    private void HandleDialogueStart()
    {
        isDialogueActive = true;
        _player?.StopImmediate();
    }

    private void HandleDialogueComplete()
    {
        isDialogueActive = false;
        _isLineDisplaying = false;
    }

    public void SetPopupActive(bool active)
    {
        isPopupActive = active;
        if (active)
        {
            _player?.StopImmediate();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- IActionMarkupHandler 구현 ---
    public void OnPrepareForLine(MarkupParseResult line, TMP_Text text) { _isLineDisplaying = true; }
    public void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text) { _isLineDisplaying = true; }
    public YarnTask OnCharacterWillAppear(int i, MarkupParseResult l, CancellationToken c) => YarnTask.CompletedTask;
    public void OnLineDisplayComplete() { _isLineDisplaying = false; }
    public void OnLineWillDismiss() { _isLineDisplaying = false; }
}
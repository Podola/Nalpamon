using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 플레이어 캐릭터의 이동, 점프, 상호작용을 처리하는 핵심 컨트롤러입니다.
/// ISaveable 인터페이스를 구현하여 플레이어의 상태를 저장하고 복원합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, ISaveable
{
    [Header("Visual & Animation")]
    public Transform VisualRoot;
    public Animator RigAnimator;

    [Header("Physics & Movement")]
    public Rigidbody2D Rb;
    public float WalkSpeed = 4f;
    public float RunSpeed = 7f;

    [Tooltip("공중에서의 좌우 이동 감도 (1이면 지상과 동일)")]
    [Range(0, 1)]
    public float AirControlFactor = 1f;

    [Header("Jump")]
    public float JumpVelocity = 11f;
    public float CoyoteTime = 0.1f;
    public float JumpBuffer = 0.1f;
    public float GravityScale = 3f;
    public float FallGravityScale = 4.5f;
    public float LowJumpGravityScale = 5f;

    [Header("Ground Check")]
    public LayerMask GroundMask;
    public Transform GroundCheck;
    public float GroundRadius = 0.15f;

    [Header("Interaction")]
    public Collider2D SensorCol;

    [Header("Events")]
    public UnityEvent OnJump;
    public UnityEvent OnLand;

    // 내부 상태 변수
    private float _axis;
    private bool _runHeld;
    private bool _facingRight = true;

    [ReadOnly]
    public bool _isGrounded;
    public bool _wasGrounded;
    public float _lastGroundTime;
    private float _lastJumpPressedTime;
    private readonly List<IInteractable> _candidates = new();
    private IInteractable _currentInteractable;

    // 애니메이터 파라미터 해시 ID (성능 최적화)
    private static readonly int AnimID_Speed = Animator.StringToHash("Speed");
    private static readonly int AnimID_IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int AnimID_IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int AnimID_VelY = Animator.StringToHash("VelY");
    private static readonly int AnimID_Jump = Animator.StringToHash("Jump");

    private void Start() // 이제 일반 void Start() 메서드를 사용합니다.
    {
        if (SaveLaunchIntent.IsNewGame)
        {
            RoomType startRoom = SaveLaunchIntent.NewGameRoom;
            string startKey = SaveLaunchIntent.NewGameTargetKey;

            Transform startPoint = TargetPointRegistry.Get(startRoom, startKey);

            if (startPoint != null)
            {
                transform.position = startPoint.position;
            }
            else
            {
                Log.E($"시작 위치를 찾지 못했습니다! Room: '{startRoom}', Key: '{startKey}'.");
            }

            SaveLaunchIntent.ClearNewGameState();
        }
    }

    void Reset()
    {
        Rb = GetComponent<Rigidbody2D>();
        if (VisualRoot == null) VisualRoot = transform.Find("Visual");
        if (RigAnimator == null) RigAnimator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        if (Rb != null) Rb.gravityScale = GravityScale;
    }

    void Update()
    {
        CheckGround();
        TryConsumeJumpBuffer();
        UpdateFacingDirection();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (Rb == null) return;
        ApplyMovement();
        ApplyBetterJumpGravity();
    }

    void LateUpdate()
    {
        UpdateNearestInteractable();
    }

    // --- Public Control Methods ---

    public void Move(float axis) => _axis = axis;
    public void SetRunning(bool runHeld) => _runHeld = runHeld;
    public void Jump() => _lastJumpPressedTime = Time.time;
    public void Interact()
    {
        if (!_isGrounded) return;
        _currentInteractable?.Interact(transform);
    }

    public void StopImmediate()
    {
        _axis = 0f;
        if (Rb != null) Rb.linearVelocity = new Vector2(0, Rb.linearVelocity.y);
    }

    // --- Internal Logic ---

    private void CheckGround()
    {
        _wasGrounded = _isGrounded;
        if (GroundCheck != null)
        {
            _isGrounded = Physics2D.OverlapCircle(GroundCheck.position, GroundRadius, GroundMask);
        }
        else
        {
            var hit = Physics2D.Raycast(transform.position + Vector3.up * 0.1f, Vector2.down, 0.2f, GroundMask);
            _isGrounded = hit.collider != null;
        }

        if (_isGrounded) _lastGroundTime = Time.time;
        if (!_wasGrounded && _isGrounded) OnLand?.Invoke();
    }

    private void TryConsumeJumpBuffer()
    {
        if (Rb == null) return;
        bool buffered = (Time.time - _lastJumpPressedTime) <= JumpBuffer;
        bool coyoteOk = (Time.time - _lastGroundTime) <= CoyoteTime;

        if (buffered && (_isGrounded || coyoteOk))
        {
            _lastJumpPressedTime = -999f;
            Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, JumpVelocity);
            RigAnimator?.SetTrigger(AnimID_Jump);
            OnJump?.Invoke();
        }
    }

    private void ApplyMovement()
    {
        float maxSpeed = _runHeld ? RunSpeed : WalkSpeed;
        float targetVX = _axis * maxSpeed;

        // 공중 제어력을 적용합니다.
        if (!_isGrounded)
        {
            targetVX *= AirControlFactor;
        }

        // 가속/감속 없이 속도를 직접 설정하여 즉각적인 반응성을 만듭니다.
        Rb.linearVelocity = new Vector2(targetVX, Rb.linearVelocity.y);
    }

    private void ApplyBetterJumpGravity()
    {
        if (Rb == null) return;
        if (Rb.linearVelocity.y < -0.01f) // 떨어질 때
        {
            Rb.gravityScale = FallGravityScale;
        }
        else if (Rb.linearVelocity.y > 0.01f && !Input.GetButton("Jump")) // 점프 키를 짧게 눌렀을 때
        {
            Rb.gravityScale = LowJumpGravityScale;
        }
        else // 기본 중력
        {
            Rb.gravityScale = GravityScale;
        }
    }

    private void UpdateFacingDirection()
    {
        if (Mathf.Abs(_axis) < 0.1f || VisualRoot == null) return;

        bool wantRight = _axis > 0f;
        if (wantRight != _facingRight)
        {
            _facingRight = wantRight;
            var scale = VisualRoot.localScale;
            scale.x = Mathf.Abs(scale.x) * (_facingRight ? 1 : -1);
            VisualRoot.localScale = scale;
        }
    }

    private void UpdateAnimator()
    {
        if (RigAnimator == null) return;
        float maxSpeed = _runHeld ? RunSpeed : WalkSpeed;
        float horizontalSpeed = 0f;

        if (_isGrounded)
        {
            // 지상에서는 입력 축(_axis)을 기준으로 하여 즉각적인 애니메이션 반응을 유도
            horizontalSpeed = Mathf.Abs(_axis);
        }
        else
        {
            // 공중에서는 실제 물리 속도를 기준으로 하되, 0으로 나누는 것을 방지
            if (maxSpeed > 0)
            {
                horizontalSpeed = Mathf.Abs(Rb.linearVelocity.x / maxSpeed);
            }
        }

        RigAnimator.SetFloat(AnimID_Speed, horizontalSpeed);
        RigAnimator.SetBool(AnimID_IsRunning, _runHeld);
        RigAnimator.SetBool(AnimID_IsGrounded, _isGrounded);
        RigAnimator.SetFloat(AnimID_VelY, Rb.linearVelocity.y);
    }

    // --- Interaction Logic ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            if (!_candidates.Contains(interactable))
                _candidates.Add(interactable);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            _candidates.Remove(interactable);
            if (_currentInteractable == interactable)
            {
                interactable.ShowIcon(false);
                _currentInteractable = null;
            }
        }
    }

    private void UpdateNearestInteractable()
    {
        // 비활성화되거나 사라진 상호작용 후보를 리스트에서 제거합니다.
        _candidates.RemoveAll(item => item == null || !(item as MonoBehaviour).gameObject.activeInHierarchy);

        IInteractable nearest = null;
        float minSqrDist = float.MaxValue;
        Vector3 p = transform.position;

        foreach (var candidate in _candidates)
        {
            // 후보가 여전히 유효한지 다시 한번 확인합니다.
            var candidateMonoBehaviour = candidate as MonoBehaviour;
            if (candidateMonoBehaviour == null) continue;

            float sqrDist = (candidateMonoBehaviour.transform.position - p).sqrMagnitude;
            if (sqrDist < minSqrDist)
            {
                minSqrDist = sqrDist;
                nearest = candidate;
            }
        }

        if (nearest != _currentInteractable)
        {
            _currentInteractable?.ShowIcon(false);
            _currentInteractable = nearest;

            if (_currentInteractable != null)
            {
                _currentInteractable.UpdateIcon();
                _currentInteractable.ShowIcon(true);
            }
        }
        // 현재 상호작용 대상이 유효하다면 아이콘 상태를 계속 업데이트합니다.
        else if (_currentInteractable != null)
        {
            _currentInteractable.UpdateIcon();
        }
    }

    // --- ISaveable 구현 ---

    public string UniqueId => "Player";

    [Serializable]
    private struct PlayerStateData
    {
        public Vector3 position;
        public bool facingRight;
    }

    public object CaptureState()
    {
        return new PlayerStateData
        {
            position = transform.position,
            facingRight = _facingRight
        };
    }

    public void RestoreState(object state)
    {
        if (state is PlayerStateData data)
        {
            StopImmediate();
            Rb.position = data.position; // Rigidbody 위치 설정
            transform.position = data.position;

            _facingRight = data.facingRight;
            if (VisualRoot != null)
            {
                var scale = VisualRoot.localScale;
                scale.x = Mathf.Abs(scale.x) * (_facingRight ? 1 : -1);
                VisualRoot.localScale = scale;
            }
        }
    }
}
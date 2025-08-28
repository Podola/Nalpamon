// NPCLocomotion2D.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public sealed class NPCLocomotion2D : MonoBehaviour
{
    // 이동 백엔드 모드
    public enum MoveBackend { Auto, Rigidbody2D, TransformOnly }

    [Header("백엔드 선택")]
    public MoveBackend Backend = MoveBackend.Auto; // Auto는 Rb 존재 여부로 결정

    [Header("필수")]
    public Rigidbody2D Rb;                 // 없으면 Transform 모드로 동작
    public Transform VisualRoot;           // 좌우 반전용 자식
    public Animator Animator;              // 애니메이션 컨트롤러

    [Header("이동")]
    public float WalkSpeed = 3.5f;         // 걷기 속도
    public float RunSpeed = 6.0f;         // 달리기 속도
    public float Accel = 30f;          // 가속
    public float Decel = 40f;          // 감속
    public float AirControl = 0.5f;        // 공중 조작 비율(TransformOnly에도 동일 가중치 적용)

    [Header("지면 판정")]
    public LayerMask GroundMask;
    public Transform GroundCheck;
    public float GroundRadius = 0.15f;

    [Header("바닥 스냅")]
    public bool SnapToGroundWhileMoving = true;
    public float SnapRayLength = 2.0f;
    public float LiftY = 0.02f;

    [Header("애니메이터 파라미터 이름")]
    public string P_Speed = "Speed";
    public string P_IsGrounded = "IsGrounded";
    public string P_IsRunning = "IsRunning";
    public string P_VelY = "VelY";
    public string T_Emote = "Emote";   // 선택적 연출용

    [Header("애니메이터 세이프 모드")]
    public bool AnimatorSafeMode = true;
    public bool LogMissingAnimParamsOnce = false;

    [Header("이벤트 훅")]
    public UnityEvent OnStartMove;   // 이동 시작
    public UnityEvent OnArrive;      // 목적지 도착
    public UnityEvent OnStartRun;    // 런 시작
    public UnityEvent OnStopRun;     // 런 종료
    public UnityEvent OnFootstep;    // 발소리. 애니메이션 이벤트에서 호출해도 됨

    // 내부 상태
    bool _isGrounded, _wasGrounded;
    bool _facingRight = true;
    bool _run;
    float _axis;                 // 내부 이동 축 -1..1
    float _vx;                   // TransformOnly용 가상 수평 속도
    float _prevY;                // TransformOnly에서 VelY 추정용
    Coroutine _moveCo;

    // 애니메이터 파라미터 캐시
    RuntimeAnimatorController _cachedCtrl;
    bool _paramsCached, _missingLogged;
    bool _hasP_Speed, _hasP_IsGrounded, _hasP_IsRunning, _hasP_VelY, _hasT_Emote;

    // 현재 모드가 물리인지 여부
    bool UsePhysics => (Backend == MoveBackend.Rigidbody2D) || (Backend == MoveBackend.Auto && Rb != null);

    void Reset()
    {
        Rb = GetComponent<Rigidbody2D>();
        if (VisualRoot == null)
        {
            var v = transform.Find("Visual") ?? transform.Find("VisualRoot");
            if (v) VisualRoot = v;
        }
        if (Animator == null) Animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        CacheAnimatorParamsIfNeeded(true);
        _prevY = transform.position.y;
    }

    void Update()
    {
        CacheAnimatorParamsIfNeeded();
        CheckGround();

        // 애니메이터 반영
        if (Animator != null)
        {
            float speedAbs = UsePhysics
                ? (Rb ? Mathf.Abs(Rb.linearVelocity.x) : Mathf.Abs(_axis * CurrentSpeed()))
                : Mathf.Abs(_vx);

            SafeSetFloat(P_Speed, speedAbs);
            SafeSetBool(P_IsGrounded, _isGrounded);
            SafeSetBool(P_IsRunning, _run);

            float velY = UsePhysics
                ? (Rb ? Rb.linearVelocity.y : 0f)
                : ((transform.position.y - _prevY) / Mathf.Max(Time.deltaTime, 1e-5f));
            SafeSetFloat(P_VelY, velY);
        }

        _prevY = transform.position.y;

        // 좌우 반전
        if (Mathf.Abs(_axis) > 0.001f && VisualRoot != null)
        {
            bool wantRight = _axis > 0f;
            if (wantRight != _facingRight)
            {
                var s = VisualRoot.localScale;
                float ax = Mathf.Abs(s.x);
                s.x = wantRight ? ax : -ax;
                VisualRoot.localScale = s;
                _facingRight = wantRight;
            }
        }
    }

    void FixedUpdate()
    {
        if (UsePhysics)
            FixedUpdate_Physics();
        else
            FixedUpdate_Transform();
    }

    // 물리 백엔드
    void FixedUpdate_Physics()
    {
        if (Rb == null) return;

        float targetVX = _axis * CurrentSpeed();
        float vx = Rb.linearVelocity.x;
        float diff = targetVX - vx;

        float a = _isGrounded ? Accel : Accel * AirControl;
        float d = _isGrounded ? Decel : Decel * AirControl;

        if (Mathf.Abs(targetVX) > 0.01f)
        {
            float delta = Mathf.Clamp(diff, -a, a) * Time.fixedDeltaTime;
            Rb.linearVelocity = new Vector2(vx + delta, Rb.linearVelocity.y);
        }
        else
        {
            float sign = Mathf.Sign(vx);
            float mag = Mathf.Max(Mathf.Abs(vx) - d * Time.fixedDeltaTime, 0f);
            Rb.linearVelocity = new Vector2(mag * sign, Rb.linearVelocity.y);
        }

        if (SnapToGroundWhileMoving && Mathf.Abs(_axis) > 0.01f)
            SnapToGround();
    }

    // 트랜스폼 백엔드
    void FixedUpdate_Transform()
    {
        float targetVX = _axis * CurrentSpeed();
        float diff = targetVX - _vx;

        float a = _isGrounded ? Accel : Accel * AirControl;
        float d = _isGrounded ? Decel : Decel * AirControl;

        if (Mathf.Abs(targetVX) > 0.01f)
        {
            float delta = Mathf.Clamp(diff, -a, a) * Time.fixedDeltaTime;
            _vx += delta;
        }
        else
        {
            float sign = Mathf.Sign(_vx);
            float mag = Mathf.Max(Mathf.Abs(_vx) - d * Time.fixedDeltaTime, 0f);
            _vx = mag * sign;
        }

        // 위치 갱신
        var pos = transform.position;
        pos.x += _vx * Time.fixedDeltaTime;
        transform.position = pos;

        if (SnapToGroundWhileMoving && Mathf.Abs(_axis) > 0.01f)
            SnapToGround();
    }

    // 바닥 스냅 공통 처리
    void SnapToGround()
    {
        Vector3 pos = transform.position;
        var hit = Physics2D.Raycast(pos + Vector3.up * 0.3f, Vector2.down, Mathf.Max(0.5f, SnapRayLength), GroundMask);
        if (hit.collider) transform.position = new Vector3(pos.x, hit.point.y + LiftY, pos.z);
    }

    // 외부 API

    // 달리기 상태 토글
    public void SetRunning(bool run)
    {
        if (_run == run) return;
        _run = run;
        if (run) OnStartRun?.Invoke();
        else OnStopRun?.Invoke();
    }

    // 즉시 정지
    public void StopImmediate()
    {
        _axis = 0f;
        _vx = 0f;
        if (Rb) Rb.linearVelocity = new Vector2(0f, Rb.linearVelocity.y);
    }

    // 한 방향 바라보기만 수행
    public void FaceRight(bool right)
    {
        if (VisualRoot == null) return;
        var s = VisualRoot.localScale;
        float ax = Mathf.Abs(s.x);
        s.x = right ? ax : -ax;
        VisualRoot.localScale = s;
        _facingRight = right;
    }

    // 월드 좌표로 걸어서 이동
    public Coroutine WalkTo(Vector3 worldPos, bool run = false, float stoppingDistance = 0.05f)
    {
        SetRunning(run);
        if (_moveCo != null) StopCoroutine(_moveCo);
        _moveCo = StartCoroutine(WalkRoutine(worldPos, stoppingDistance));
        return _moveCo;
    }

    // 트랜스폼으로 걸어서 이동
    public Coroutine WalkTo(Transform target, bool run = false, float stoppingDistance = 0.05f)
    {
        if (target == null) return null;
        return WalkTo(target.position, run, stoppingDistance);
    }

    // TargetPointRegistry 키로 걸어서 이동
    public Coroutine WalkTo(RoomType room, string key, bool run = false, float stoppingDistance = 0.05f)
    {
        var t = TargetPointRegistry.Get(room, key);
        if (t == null) return null;
        return WalkTo(t.position, run, stoppingDistance);
    }

    // 애니메이션 이벤트에서 호출할 수 있는 발소리
    public void Footstep()
    {
        OnFootstep?.Invoke();
    }

    // 내부 코루틴

    IEnumerator WalkRoutine(Vector3 worldPos, float stopDist)
    {
        OnStartMove?.Invoke();

        // Y는 현재 높이를 유지하고 X만 맞춘다
        Vector3 target = new Vector3(worldPos.x, transform.position.y, transform.position.z);

        // 방향 결정
        _axis = Mathf.Sign(target.x - transform.position.x);
        if (Mathf.Abs(_axis) < 0.01f) _axis = 0f;

        // 도착 판정 루프
        while (true)
        {
            float dx = target.x - transform.position.x;
            if (Mathf.Abs(dx) <= stopDist)
                break;

            _axis = Mathf.Sign(dx);
            yield return null;
        }

        // 정지
        _axis = 0f;
        StopImmediate();
        OnArrive?.Invoke();
        _moveCo = null;
    }

    // 보조

    void CheckGround()
    {
        _wasGrounded = _isGrounded;

        if (GroundCheck != null)
            _isGrounded = Physics2D.OverlapCircle(GroundCheck.position, GroundRadius, GroundMask);
        else
        {
            Vector2 origin = transform.position;
            var hit = Physics2D.Raycast(origin + Vector2.up * 0.1f, Vector2.down, 0.2f, GroundMask);
            _isGrounded = hit.collider != null;
        }
    }

    float CurrentSpeed() => _run ? RunSpeed : WalkSpeed;

    // 애니메이터 세이프 모드

    void CacheAnimatorParamsIfNeeded(bool force = false)
    {
        if (!AnimatorSafeMode || Animator == null) return;

        var ctrl = Animator.runtimeAnimatorController;
        if (!force && _paramsCached && _cachedCtrl == ctrl) return;

        _cachedCtrl = ctrl;
        _paramsCached = true;

        _hasP_Speed = HasParam(P_Speed, AnimatorControllerParameterType.Float);
        _hasP_IsGrounded = HasParam(P_IsGrounded, AnimatorControllerParameterType.Bool);
        _hasP_IsRunning = HasParam(P_IsRunning, AnimatorControllerParameterType.Bool);
        _hasP_VelY = HasParam(P_VelY, AnimatorControllerParameterType.Float);
        _hasT_Emote = HasParam(T_Emote, AnimatorControllerParameterType.Trigger);

        if (LogMissingAnimParamsOnce && !_missingLogged)
        {
            var missing = new System.Collections.Generic.List<string>();
            if (!_hasP_Speed) missing.Add(P_Speed);
            if (!_hasP_IsGrounded) missing.Add(P_IsGrounded);
            if (!_hasP_IsRunning) missing.Add(P_IsRunning);
            if (!_hasP_VelY) missing.Add(P_VelY);
            if (!_hasT_Emote) missing.Add(T_Emote);

            if (missing.Count > 0)
                Debug.LogWarning("[NPCLocomotion2D] Animator에 없는 파라미터: " + string.Join(", ", missing));

            _missingLogged = true;
        }
    }

    bool HasParam(string name, AnimatorControllerParameterType type)
    {
        if (Animator == null || string.IsNullOrEmpty(name)) return false;
        foreach (var p in Animator.parameters)
            if (p.type == type && p.name == name)
                return true;
        return false;
    }

    void SafeSetFloat(string name, float v)
    {
        if (Animator == null) return;
        if (!AnimatorSafeMode) { Animator.SetFloat(name, v); return; }
        if ((name == P_Speed && _hasP_Speed) || (name == P_VelY && _hasP_VelY))
            Animator.SetFloat(name, v);
    }

    void SafeSetBool(string name, bool v)
    {
        if (Animator == null) return;
        if (!AnimatorSafeMode) { Animator.SetBool(name, v); return; }
        if ((name == P_IsGrounded && _hasP_IsGrounded) || (name == P_IsRunning && _hasP_IsRunning))
            Animator.SetBool(name, v);
    }

    public void SafeSetTrigger(string name)
    {
        if (Animator == null) return;
        if (!AnimatorSafeMode) { Animator.SetTrigger(name); return; }
        if (name == T_Emote && _hasT_Emote)
            Animator.SetTrigger(name);
    }
}

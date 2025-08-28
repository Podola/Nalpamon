// NpcStepPlacement.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 스텝 전환에 따라 NPC의 위치를 배치하는 컴포넌트
// 타겟 Transform 또는 TargetPointRegistry를 사용해 위치를 정하고
// 배치 직후 VisualRoot의 좌우도 함께 정렬한다
public class NPCStepPlacement : MonoBehaviour
{
    // 언제 적용할지 정의
    public enum ApplyWhen
    {
        OnEnter,             // 스텝 진입 시 즉시 적용
        AfterEntryDialogue,  // 스텝 진입 대사가 끝난 직후 적용
        OnExitPrev           // 이전 스텝에서 나갈 때 적용
    }

    // 엔트리 하나가 한 스텝에서의 배치를 정의한다
    [System.Serializable]
    public class Entry
    {
        [Header("타이밍")]
        public StepId step = StepId.None;      // 대상 스텝
        public ApplyWhen when = ApplyWhen.OnEnter;

        [Header("타겟 결정 방식")]
        public Transform direct;               // 직접 Transform로 배치
        public RoomType room = RoomType.None;  // TargetPoint의 룸
        public string key;                     // TargetPoint 키
        public bool preferDirect = true;       // true면 direct 우선, false면 TP 우선

        [Header("배치 옵션")]
        public Vector3 offset;
        public bool useTargetRotation = false;
        public bool snapToGround = true;
        public LayerMask groundMask;
        public float groundRay = 4f;
        public float liftY = 0.02f;

        [Header("이동 옵션")]
        public bool move = false;              // 부드럽게 이동할지 여부
        public float moveSeconds = 0f;         // 이동 시간. 0이면 즉시

        [Header("타겟 없음 대응")]
        public bool hideIfMissing = false;     // 타겟을 못 찾으면 오브젝트 비활성
    }

    [Header("엔트리 목록")]
    public List<Entry> entries = new List<Entry>();

    [Header("좌우 정렬")]
    // NPC의 자식 Visual을 인스펙터에서 바인딩
    public Transform VisualRoot;
    public bool adoptFacingFromTarget = true;   // 타겟의 좌우 힌트를 채택
    public bool invertAdoptedFacing = false;    // 필요시 반전

    Coroutine _moveRoutine;
    StepManager _step;
    bool _subscribed;

    void OnEnable()
    {
        // GameEvents 구독
        GameEvents.OnStepChanged += HandleStepChanged;
        GameEvents.OnMarkChanged += HandleMarkChanged;

        // 현재 스텝 정보는 StepManager에서 직접 가져옴
        var currentStep = StepManager.Instance != null ? StepManager.Instance.CurrentStep : StepId.None;
        TryApplyFor(currentStep, ApplyWhen.OnEnter);
    }

    void OnDisable()
    {
        // GameEvents 구독 해제
        GameEvents.OnStepChanged -= HandleStepChanged;
        GameEvents.OnMarkChanged -= HandleMarkChanged;

        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }
    }

    // 스텝이 바뀌면 OnExitPrev에 해당하는 이전 스텝 엔트리를 먼저 적용
    // 그리고 새 스텝의 OnEnter 엔트리를 적용
    void HandleStepChanged(StepId prev, StepId next)
    {
        TryApplyFor(prev, ApplyWhen.OnExitPrev);
        TryApplyFor(next, ApplyWhen.OnEnter);
    }

    // 스텝 진입 대사가 끝나면 StepEntryKey가 true가 되므로
    // 해당 마크를 감지해서 AfterEntryDialogue를 적용
    void HandleMarkChanged(string key, bool value)
    {
        if (!_subscribed || _step == null || !value) return;
        if (key == Util.StepEntryKey(_step.CurrentStep))
        {
            TryApplyFor(_step.CurrentStep, ApplyWhen.AfterEntryDialogue);
        }
    }

    // 특정 스텝의 특정 타이밍에 해당하는 모든 엔트리를 적용
    void TryApplyFor(StepId step, ApplyWhen when)
    {
        if (step == StepId.None) return;
        if (entries == null || entries.Count == 0) return;

        for (int i = 0; i < entries.Count; ++i)
        {
            var e = entries[i];
            if (e == null) continue;
            if (e.step != step) continue;
            if (e.when != when) continue;

            ApplyEntry(e);
        }
    }

    // 엔트리 하나를 적용한다
    void ApplyEntry(Entry e)
    {
        // 타겟 Transform 결정
        Transform dst = null;

        // preferDirect의 우선순위에 따라 direct와 TargetPoint를 고른다
        if (e.preferDirect)
        {
            if (e.direct != null) dst = e.direct;
            else if (!string.IsNullOrEmpty(e.key) && e.room != RoomType.None)
                dst = TargetPointRegistry.Get(e.room, e.key);
        }
        else
        {
            if (!string.IsNullOrEmpty(e.key) && e.room != RoomType.None)
                dst = TargetPointRegistry.Get(e.room, e.key);
            if (dst == null && e.direct != null) dst = e.direct;
        }

        // 타겟을 못 찾았을 때의 대응
        if (dst == null)
        {
            if (e.hideIfMissing) gameObject.SetActive(false);
            return;
        }
        else
        {
            if (e.hideIfMissing && !gameObject.activeSelf) gameObject.SetActive(true);
        }

        // 목표 위치 계산
        Vector3 pos = dst.position + e.offset;

        // 바닥 스냅. 상하 방향 둘 다 시도해 허용 오차 내에서 정렬
        if (e.snapToGround)
        {
            var up = pos + Vector3.up * 0.5f;
            var dn = pos + Vector3.down * 0.5f;
            var hit = Physics2D.Raycast(up, Vector2.down, Mathf.Max(1f, e.groundRay), e.groundMask);
            if (!hit.collider)
                hit = Physics2D.Raycast(dn, Vector2.up, Mathf.Max(1f, e.groundRay), e.groundMask);
            if (hit.collider) pos.y = hit.point.y + (e.liftY <= 0 ? 0.02f : e.liftY);
        }

        // 목표 회전 계산
        Quaternion rot = e.useTargetRotation ? dst.rotation : transform.rotation;

        // 이동 처리. moveSeconds가 0이면 즉시 세팅
        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }

        if (!e.move || e.moveSeconds <= 0f || !gameObject.activeInHierarchy)
            transform.SetPositionAndRotation(pos, rot);
        else
            _moveRoutine = StartCoroutine(MoveRoutine(pos, rot, e.moveSeconds));

        // 좌우 정렬. 타겟의 localScale.x 부호를 우선 채택한다
        if (adoptFacingFromTarget)
            ApplyFacingFromTarget(dst);
    }

    // 타겟 Transform로부터 좌우 힌트를 읽어 VisualRoot의 localScale.x를 조절
    void ApplyFacingFromTarget(Transform dst)
    {
        if (VisualRoot == null || dst == null) return;

        // 우선순위는 localScale.x 부호. 0이면 회전의 right.x를 사용
        float hint = dst.localScale.x;
        if (Mathf.Approximately(hint, 0f))
        {
            float rx = dst.right.x;
            if (Mathf.Abs(rx) > 1e-4f) hint = Mathf.Sign(rx);
        }

        if (Mathf.Approximately(hint, 0f)) return;

        bool faceRight = hint > 0f;
        if (invertAdoptedFacing) faceRight = !faceRight;

        var s = VisualRoot.localScale;
        float ax = Mathf.Abs(s.x);
        s.x = faceRight ? ax : -ax;
        VisualRoot.localScale = s;
    }

    // 부드러운 이동 코루틴
    IEnumerator MoveRoutine(Vector3 toPos, Quaternion toRot, float seconds)
    {
        var fromPos = transform.position;
        var fromRot = transform.rotation;

        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / seconds);
            transform.position = Vector3.Lerp(fromPos, toPos, a);
            transform.rotation = Quaternion.Slerp(fromRot, toRot, a);
            yield return null;
        }
        transform.SetPositionAndRotation(toPos, toRot);
        _moveRoutine = null;
    }
}

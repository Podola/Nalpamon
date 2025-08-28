// 파일 경로: Scripts/2_GamePlay/World/Rooms/ParallaxSystem.cs

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Cinemachine;

[ExecuteAlways]
public class ParallaxSystem : MonoBehaviour
{
    [Header("Sorting 설정 (에디터 전용)")]
    public string GroundPrefix = "GRD_";
    public string ForegroundPrefix = "FG_";
    public string BackgroundPrefix = "BG_";
    public string FarBackgroundPrefix = "BF_";

    public string GroundLayer = "Ground";
    public string ForegroundLayer = "Foreground";
    public string MidgroundLayer = "Default";
    public string BackgroundLayer = "Background";
    public string FarBackgroundLayer = "Background_Far";


    [Header("Parallax 설정")]
    [Range(-2f, 2f)] public float ForegroundAmount = 1.15f;
    [Range(-2f, 2f)] public float BackgroundAmount = 0.4f;
    [Range(-2f, 2f)] public float FarBackgroundAmount = 0.6f;

    // (이하 나머지 코드는 이전 버전과 동일합니다)

    [SerializeField, HideInInspector]
    private List<ParallaxItem> _parallaxItems = new List<ParallaxItem>();

    [System.Serializable]
    private struct ParallaxItem { public Transform Target; public Vector3 StartPos; public float Amount; }

    private CinemachineBrain _cinemachineBrain;
    private CinemachineCamera _linkedVCam;
    private Transform _artRoot;
    private Vector3 _camAnchorPos;
    private bool _isCurrentlyActive = false;
    private bool _isInitialized = false;

    void Awake() { _artRoot = transform; }

    void OnEnable()
    {
        if (!Application.isPlaying) return;
        Initialize();
    }

    void LateUpdate()
    {
        if (!_isInitialized || !Application.isPlaying) return;
        bool isNowLive = (_cinemachineBrain.ActiveVirtualCamera as CinemachineCamera) == _linkedVCam;
        if (isNowLive && !_isCurrentlyActive) CaptureParallaxAnchors();
        _isCurrentlyActive = isNowLive;
        if (_isCurrentlyActive) ApplyParallax();
    }

    private void Initialize()
    {
        if (_isInitialized) return;
        _cinemachineBrain = Camera.main?.GetComponent<CinemachineBrain>();
        var roomController = GetComponentInParent<RoomController>();
        if (roomController != null)
        {
            var stateDrivenCam = FindFirstObjectByType<CinemachineStateDrivenCamera>();
            if (stateDrivenCam != null)
            {
                string vcamName = $"VCam_{roomController.gameObject.name}";
                _linkedVCam = stateDrivenCam.transform.Find(vcamName)?.GetComponent<CinemachineCamera>();
            }
        }
        _isInitialized = _artRoot != null && _cinemachineBrain != null && _linkedVCam != null;
    }

    private void CaptureParallaxAnchors()
    {
        _camAnchorPos = _cinemachineBrain.transform.position;
        for (int i = 0; i < _parallaxItems.Count; i++)
        {
            var item = _parallaxItems[i];
            if (item.Target != null)
            {
                item.StartPos = item.Target.position;
                _parallaxItems[i] = item;
            }
        }
    }

    private void ApplyParallax()
    {
        var camDelta = _cinemachineBrain.transform.position - _camAnchorPos;
        for (int i = 0; i < _parallaxItems.Count; i++)
        {
            var item = _parallaxItems[i];
            if (item.Target == null) continue;
            float parallaxX = item.StartPos.x + (camDelta.x * item.Amount);
            item.Target.position = new Vector3(parallaxX, item.StartPos.y, item.StartPos.z);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("1. [Bake] Apply Sorting by Prefix")]
    private void ApplySorting()
    {
        if (_artRoot == null) _artRoot = transform;
        UnityEditor.Undo.RecordObjects(_artRoot.GetComponentsInChildren<Renderer>(true), "Apply Sorting");

        var renderers = _artRoot.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            // --- 여기가 수정된 부분입니다 ---
            // GRD_ 접두사를 가장 먼저 확인하여 최우선으로 처리합니다.
            if (r.name.StartsWith(GroundPrefix)) SetRendererSorting(r, GroundLayer);
            else if (r.name.StartsWith(ForegroundPrefix)) SetRendererSorting(r, ForegroundLayer);
            else if (r.name.StartsWith(BackgroundPrefix)) SetRendererSorting(r, BackgroundLayer);
            else if (r.name.StartsWith(FarBackgroundPrefix)) SetRendererSorting(r, FarBackgroundLayer);
            else SetRendererSorting(r, MidgroundLayer); // 접두사가 없으면 기본값

            UnityEditor.EditorUtility.SetDirty(r);
        }
        Log.I($"[{gameObject.name}] Sorting 적용 완료. ({renderers.Length}개)");
    }

    [ContextMenu("2. [Bake] Collect Parallax Items")]
    private void CollectParallaxItems()
    {
        if (_artRoot == null) _artRoot = transform;
        UnityEditor.Undo.RecordObject(this, "Collect Parallax Items");
        _parallaxItems.Clear();

        var renderers = _artRoot.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            float amount = 0;
            // Ground는 플레이어와 같은 평면에 있으므로 패럴랙스 효과를 주지 않습니다. (amount = 0)
            if (r.name.StartsWith(ForegroundPrefix)) amount = ForegroundAmount;
            else if (r.name.StartsWith(BackgroundPrefix)) amount = BackgroundAmount;
            else if (r.name.StartsWith(FarBackgroundPrefix)) amount = FarBackgroundAmount;

            if (Mathf.Abs(amount) > 0.001f)
            {
                _parallaxItems.Add(new ParallaxItem { Target = r.transform, Amount = amount });
            }
        }
        UnityEditor.EditorUtility.SetDirty(this);
        Log.I($"[{gameObject.name}] Parallax 대상 수집 완료. ({_parallaxItems.Count}개)");
    }

    private void SetRendererSorting(Renderer r, string layerName)
    {
        var group = r.GetComponentInParent<SortingGroup>();
        if (group != null && group.transform != r.transform)
        {
            group.sortingLayerName = layerName;
        }
        else
        {
            r.sortingLayerName = layerName;
        }
    }
#endif
}
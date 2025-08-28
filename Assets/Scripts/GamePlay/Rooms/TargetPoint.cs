// 파일 경로: Scripts/2_GamePlay/World/Rooms/TargetPoint.cs
using UnityEngine;

// ※ RoomType Enum은 프로젝트의 다른 파일에 정의되어 있다고 가정합니다.
// public enum RoomType { None, Town, Forest, Cave, ... }

public enum TargetPointType
{
    Default,
    PlayerStart,
    WarpDestination,
    PartyFollow, // 파티원 전용 배치 지점
    NPCPlacement // 일반 NPC 스텝별 배치 지점
}

[ExecuteAlways]
public sealed class TargetPoint : MonoBehaviour
{
    [Header("식별 정보")]
    public RoomType room;
    public TargetPointType type = TargetPointType.NPCPlacement;

    [Tooltip("Type이 PartyFollow가 아닐 때 사용할 고유 키")]
    public string key = "Default";

    [Tooltip("Type이 PartyFollow일 때, 따라올 파티원의 ID")]
    public NpcId partyFollowerId = NpcId.None;

#if UNITY_EDITOR
    [Header("Editor Visualization")]
    [SerializeField] private Color gizmoColor = Color.green;

    private void OnValidate()
    {
        string targetName;

        switch (type)
        {
            case TargetPointType.PlayerStart:
                gizmoColor = Color.cyan;
                targetName = $"TP_{room}_PlayerStart";
                break;

            case TargetPointType.WarpDestination:
                gizmoColor = Color.yellow;
                // [변경] 이름 형식을 "TP_{목적지}_Warp_From_{출발지}"로 변경
                // key 프로퍼티가 출발지 Room의 이름을 담게 됩니다.
                targetName = $"TP_{room}_Warp_From_{key}";
                break;

            case TargetPointType.PartyFollow:
                gizmoColor = new Color(1f, 0.5f, 0f); // Orange
                targetName = $"TP_{room}_Party_{partyFollowerId}";
                break;

            case TargetPointType.NPCPlacement:
                gizmoColor = Color.magenta;
                targetName = $"TP_{room}_NPC_{key}";
                break;

            default:
                gizmoColor = Color.green;
                targetName = $"TP_{room}_{key}";
                break;
        }

        gameObject.name = targetName;
    }


    // OnDrawGizmos는 선택 여부와 관계없이 항상 기즈모를 그립니다.
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, 0.25f);
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f);
    }

    // OnDrawGizmosSelected는 오브젝트가 선택되었을 때만 호출됩니다.
    private void OnDrawGizmosSelected()
    {
        DrawLabel();
    }

    private void DrawLabel()
    {
        GUIStyle style = new GUIStyle { normal = { textColor = gizmoColor }, fontSize = 15, fontStyle = FontStyle.Bold };
        string label = (type == TargetPointType.PartyFollow) ? $"[{type}] {partyFollowerId}" : $"[{type}] {key}";
        UnityEditor.Handles.Label(transform.position + transform.up * 0.6f, label, style);
    }
#endif
}
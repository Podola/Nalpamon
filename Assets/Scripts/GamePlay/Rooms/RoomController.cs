// Scripts/2_GamePlay/World/Rooms/RoomController.cs

using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 하나의 방(Room)을 정의하고, 해당 방의 기능적 요소들을 관리합니다.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
public sealed class RoomController : MonoBehaviour
{
    [Header("Room Info")]
    public RoomType Type = RoomType.None;

    [Header("Components")]
    [Tooltip("카메라 경계의 기준이 될 아트 Transform")]
    public Transform ArtRoot;

    [HideInInspector]
    public Collider2D Boundary;

    private void Awake()
    {
        Boundary = GetComponent<Collider2D>();

        if (ArtRoot == null) ArtRoot = transform.Find("ArtRoot");
    }

    public bool Contains(Vector2 point) => Boundary != null && Boundary.OverlapPoint(point);
}
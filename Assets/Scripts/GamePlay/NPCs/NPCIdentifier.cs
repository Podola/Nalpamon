using UnityEngine;

[DisallowMultipleComponent]
public sealed class NPCIdentifier : MonoBehaviour
{
    [Header("이 NPC의 고유 ID")]
    public NpcId Id; 

    [Header("좌우 반전 기준, 비우면 첫 번째 자식 Visual을 자동 탐색")]
    public Transform VisualRoot;

    void Reset()
    {
        if (!VisualRoot)
        {
            var v = transform.Find("Visuals") ?? transform.Find("Visual");
            if (v) VisualRoot = v;
        }
    }
}
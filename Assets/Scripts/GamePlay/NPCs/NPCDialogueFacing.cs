using UnityEngine;

public sealed class NPCDialogueFacing : MonoBehaviour
{
    [Header("NPC Visual Root")]
    public Transform VisualRoot; // ← 인스펙터에서 NPC의 Visual(자식) 연결

    [Header("옵션")]
    public bool faceEachOtherOnStart = true;   // 대화 시작 시 서로 바라보게
    public bool restoreAfterEnd = true;   // 대화 종료 후 스케일 복구

    // 이름 탐색 폴백(바인딩 안 했을 때만)
    Transform ResolveNPCVisual()
    {
        if (VisualRoot != null) return VisualRoot;
        var v = transform.Find("Visual") ?? transform.Find("VisualRoot");
        return v ? v : transform;
    }

    Transform ResolvePlayerVisual(Transform playerRoot)
    {
        var pc = playerRoot.GetComponent<PlayerController>();
        if (pc != null && pc.VisualRoot != null) return pc.VisualRoot;
        var v = playerRoot.Find("Visual") ?? playerRoot.Find("VisualRoot");
        return v ? v : playerRoot;
    }

    // NPCInteractable.Interact(caller) 직전에 호출해 주세요.
    public System.Action ApplyFacingBeforeDialogue(Transform playerRoot)
    {
        if (!faceEachOtherOnStart) return null;

        var npcVis = ResolveNPCVisual();
        var plrVis = ResolvePlayerVisual(playerRoot);
        if (!npcVis || !plrVis) return null;

        // 이전 상태 저장
        var npcScalePrev = npcVis.localScale;
        var plrScalePrev = plrVis.localScale;

        // 서로 바라보게
        bool npcOnRight = npcVis.position.x > plrVis.position.x;
        Face(plrVis, npcOnRight);   // 플레이어: NPC 쪽 향함
        Face(npcVis, !npcOnRight);  // NPC: 플레이어 쪽 향함

        // 복구 액션 반환
        if (!restoreAfterEnd) return null;
        return () =>
        {
            if (npcVis) npcVis.localScale = npcScalePrev;
            if (plrVis) plrVis.localScale = plrScalePrev;
        };
    }

    static void Face(Transform visual, bool faceRight)
    {
        var s = visual.localScale;
        float ax = Mathf.Abs(s.x);
        s.x = faceRight ? ax : -ax;
        visual.localScale = s;
    }
}

// 파일 경로: Scripts/2_GamePlay/World/NPCs/NpcPlacementSystem.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NpcPlacementSystem : Singleton<NpcPlacementSystem>
{
    [Header("Party Settings")]
    public List<NpcId> partyMemberIds = new List<NpcId>();

    private readonly Dictionary<NpcId, Transform> _npcCache = new Dictionary<NpcId, Transform>();

    protected override void Awake() { base.Awake(); }
    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; GameEvents.OnStepChanged += HandleStepChanged; WarpInteractable.OnPlayerWarped += HandlePlayerWarped; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; GameEvents.OnStepChanged -= HandleStepChanged; WarpInteractable.OnPlayerWarped -= HandlePlayerWarped; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BuildNpcCache();
        if (StepManager.Instance != null)
        {
            HandleStepChanged(StepId.None, StepManager.Instance.CurrentStep);
        }
    }

    private void BuildNpcCache()
    {
        _npcCache.Clear();
        var allNpcs = FindObjectsByType<NPCIdentifier>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var npc in allNpcs)
        {
            if (npc.Id != NpcId.None && !_npcCache.ContainsKey(npc.Id))
                _npcCache.Add(npc.Id, npc.transform);
        }
    }

    private void HandleStepChanged(StepId prev, StepId next)
    {
        ProcessPlacementsFor(next);
        UpdatePartyMembersActivation(next);
    }

    private void HandlePlayerWarped(Transform playerTransform, Transform destinationPoint)
    {
        if (!IsPartyFollowActive()) return;

        var currentRoom = RoomManager.Instance.CurrentRoomType;
        foreach (var npcId in partyMemberIds)
        {
            Transform targetPoint = TargetPointRegistry.GetPartyMemberPoint(currentRoom, npcId);
            if (targetPoint != null)
            {
                PlaceNpc(npcId, targetPoint.position);
            }
            else
            {
                GetNpcTransform(npcId)?.gameObject.SetActive(false);
            }
        }
    }

    private void ProcessPlacementsFor(StepId step)
    {
        if (step == StepId.None) return;
        var config = StepManager.Instance?.GetConfig(step);
        if (config == null || config.npcPlacements == null) return;

        foreach (var placement in config.npcPlacements)
        {
            if (partyMemberIds.Contains(placement.npcId) && !IsPartyFollowActive()) continue;

            Transform targetPoint = TargetPointRegistry.Get(placement.room, placement.targetPointKey);
            if (targetPoint != null)
            {
                PlaceNpc(placement.npcId, targetPoint.position);
            }
            else
            {
                GetNpcTransform(placement.npcId)?.gameObject.SetActive(false);
            }
        }
    }

    private void PlaceNpc(NpcId npcId, Vector3 targetPos)
    {
        Transform npc = GetNpcTransform(npcId);
        if (npc == null) return;

        if (!npc.gameObject.activeSelf) npc.gameObject.SetActive(true);

        // Ground Snap 로직
        var hit = Physics2D.Raycast(new Vector2(targetPos.x, targetPos.y + 0.5f), Vector2.down, 4f, LayerMask.GetMask("Ground"));
        if (hit.collider)
        {
            targetPos.y = hit.point.y + 0.02f;
        }

        npc.position = targetPos;
    }

    private void UpdatePartyMembersActivation(StepId currentStep)
    {
        bool shouldBeActive = IsPartyFollowActive(currentStep);
        foreach (var memberId in partyMemberIds)
        {
            var member = GetNpcTransform(memberId);
            if (member != null && member.gameObject.activeSelf != shouldBeActive)
            {
                member.gameObject.SetActive(shouldBeActive);
                if (shouldBeActive)
                {
                    var dest = TargetPointRegistry.GetPartyMemberPoint(RoomManager.Instance.CurrentRoomType, memberId);

                    // [수정 전]
                    // if (dest != null) member.position = dest.position;

                    // [수정 후] PlaceNpc를 호출하여 Ground 바인딩 로직을 태웁니다.
                    if (dest != null)
                    {
                        PlaceNpc(memberId, dest.position);
                    }
                }
            }
        }
    }

    private bool IsPartyFollowActive(StepId? step = null)
    {
        var sm = StepManager.Instance;
        if (sm == null) return false;
        var currentStep = step ?? sm.CurrentStep;
        var cfg = sm.GetConfig(currentStep);
        return (cfg != null && cfg.stepMode == StepDatabaseSO.StepMode.Free);
    }

    private Transform GetNpcTransform(NpcId npcId)
    {
        if (_npcCache.TryGetValue(npcId, out Transform npc)) return npc;
        return null;
    }
}
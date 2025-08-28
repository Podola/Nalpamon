using System.Collections.Generic;
using UnityEngine;

public sealed class TargetPointRegistry : Singleton<TargetPointRegistry>
{
    private readonly Dictionary<(RoomType, string), Transform> _keyedPoints = new();
    private readonly Dictionary<(RoomType, NpcId), Transform> _partyPoints = new();

    protected override void Awake()
    {
        base.Awake();
        Rebuild();
    }

    private void Rebuild()
    {
        _keyedPoints.Clear();
        _partyPoints.Clear();
        var found = FindObjectsByType<TargetPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var p in found)
        {
            if (p.type == TargetPointType.PartyFollow && p.partyFollowerId != NpcId.None)
            {
                var key = (p.room, p.partyFollowerId);
                if (!_partyPoints.ContainsKey(key)) _partyPoints.Add(key, p.transform);
            }
            else if (!string.IsNullOrEmpty(p.key))
            {
                var key = (p.room, p.key);
                if (!_keyedPoints.ContainsKey(key)) _keyedPoints.Add(key, p.transform);
            }
        }
    }

    public static Transform Get(RoomType room, string key)
    {
        if (Instance == null || string.IsNullOrEmpty(key)) return null;
        Instance._keyedPoints.TryGetValue((room, key), out var t);
        return t;
    }

    public static Transform GetPartyMemberPoint(RoomType room, NpcId partyMemberId)
    {
        if (Instance == null || partyMemberId == NpcId.None) return null;
        Instance._partyPoints.TryGetValue((room, partyMemberId), out var t);
        return t;
    }
}
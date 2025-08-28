using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStandingInfo", menuName = "SO/Data/StandingInfo")]
public class CharStandingInfoSO : ScriptableObject
{
    public StandingSpriteInfo[] infos;

    public Dictionary<string, StandingSpriteInfo> Convert()
    {
        Dictionary<string, StandingSpriteInfo> result = new();

        foreach (StandingSpriteInfo info in infos) result.Add(info.key, info);

        return result;
    }
}
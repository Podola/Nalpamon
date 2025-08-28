using UnityEngine;

[System.Serializable]
public struct StandingMouthInfo
{
    public Sprite open;
    public Sprite wideOpen;

    public Sprite close;

    public Sprite fix;

    public readonly Sprite Get(StandingMouthType type)
    {
        Sprite result = type switch
        {
            StandingMouthType.None => null,
            StandingMouthType.Open => open,
            StandingMouthType.WideOpen => wideOpen,
            StandingMouthType.Close => close,
            StandingMouthType.Fix => fix,
            _ => throw new System.NotImplementedException(),
        };

        if (result == null && fix != null) result = fix;

        return result;
    }
}

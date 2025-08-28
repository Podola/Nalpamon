using UnityEngine;

public struct StandingDisplayInfo
{
    public string id;

    public int sibling;

    public StandingSpriteInfo spriteInfo;
    public bool right;

    public StandingDisplayInfo(string id, int sibling, StandingSpriteInfo spriteInfo, bool right)
    {
        this.id = id;

        this.sibling = sibling;

        this.spriteInfo = spriteInfo;
        this.right = right;
    }

    public readonly Sprite GetSprite(StandingMouthType type)
    {
        spriteInfo.GetSprite(out Sprite sprite, right, type);

        return sprite;
    }

    public readonly string GetSpriteID() => spriteInfo.key;

    public readonly bool TryGetSprite(out Sprite sprite, StandingMouthType type) => spriteInfo.GetSprite(out sprite, right, type);
}
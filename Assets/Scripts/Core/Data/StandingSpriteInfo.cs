using UnityEngine;

[System.Serializable]
public struct StandingSpriteInfo
{
    public string key;
    public StandingMouthInfo leftSprites;
    public StandingMouthInfo rightSprites;

    public readonly bool GetSprite(out Sprite sprite, bool right, StandingMouthType type)
    {
        if (right)
        {
            sprite = rightSprites.Get(type);

            if (sprite == null)
            {
                sprite = leftSprites.Get(type);

                return false;
            }
        }
        else
        {
            sprite = leftSprites.Get(type);

            if (sprite == null)
            {
                sprite = rightSprites.Get(type);

                return false;
            }
        }

        return true;
    }
}
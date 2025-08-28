using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class CharStandingHandle : MonoBehaviour
{
    [SerializeField] private Image image;

    private StandingDisplayInfo displayInfo;

    private StandingMouthType mouthType = StandingMouthType.None;
    private float mouthOpenTime = 0;

    private readonly float fadeDuration = 0.2f;

    public bool IsRight { get; set; }

    public string CharID { get; private set; }
    public RectTransform Rect { get; private set; }

    public string SpriteID => displayInfo.GetSpriteID();

    private int sibling;
    public int Sibling
    {
        get => sibling;
        set
        {
            if (value != -1) sibling = value;

            if (sibling == 0) image.color = new Color(0.6f, 0.6f, 0.6f, image.color.a);
            else
            {
                image.color = new Color(1, 1, 1, image.color.a);

                int index = Mathf.Clamp(transform.parent.childCount - sibling, 0, transform.parent.childCount - 1);

                transform.SetSiblingIndex(index);
            }
        }
    }

    #region 풀링
    private IObjectPool<CharStandingHandle> pool;

    public void SetPool(IObjectPool<CharStandingHandle> pool)
    {
        this.pool = pool;
        Rect = transform as RectTransform;
    }

    private void Release()
    {
        image.color = new Color32(150, 150, 150, 255);

        mouthType = StandingMouthType.Close;
        mouthOpenTime = 0;

        pool.Release(this);

        CharID = string.Empty;
        Sibling = 0;

        transform.localScale = Vector3.one;
    }
    #endregion

    public void SetInfo(StandingDisplayInfo displayInfo)
    {
        this.displayInfo = displayInfo;

        mouthType = StandingMouthType.Close;

        bool rotate = !displayInfo.TryGetSprite(out Sprite sprite, mouthType);

        SetSprite(sprite, CharID != displayInfo.id);

        IsRight = displayInfo.right;

        transform.localEulerAngles = rotate ? Vector3.up * 180 : Vector3.zero;

        CharID = displayInfo.id;
        Sibling = displayInfo.sibling;
    }

    public void ChangeMouthShape(StandingMouthType type)
    {
        if (type == StandingMouthType.None)
        {
            if (Sibling == 0)
            {
                image.color = new Color(1, 1, 1, image.color.a);

                transform.SetAsLastSibling();
            }

            mouthOpenTime += Time.deltaTime;

            if (mouthOpenTime < 0.02f) return;
            else mouthOpenTime = 0;

            switch (mouthType)
            {
                case StandingMouthType.Open:
                    mouthType = StandingMouthType.Close;
                    break;

                case StandingMouthType.Close:
                    mouthType = StandingMouthType.Open;
                    break;
            }

            SetSprite(displayInfo.GetSprite(mouthType), false);
        }
        else
        {
            if (Sibling == 0) image.color = new Color(0.6f, 0.6f, 0.6f, image.color.a);

            SetSprite(displayInfo.GetSprite(type), false);
        }
    }

    public void Rotate()
    {
        displayInfo.right = !displayInfo.right;

        bool rotate = !displayInfo.TryGetSprite(out Sprite sprite, StandingMouthType.Close);

        SetSprite(sprite, false);

        IsRight = displayInfo.right;

        transform.localEulerAngles += rotate ? Vector3.up * 180 : Vector3.zero;

    }

    public Vector2 GetConvertPosInfo(Vector4 posInfo, RectTransform parent)
    {
        float slotWidth = parent.rect.width / posInfo.x;
        float slotHeight = parent.rect.height / posInfo.z;

        return new(
            posInfo.x > 0 ? slotWidth * posInfo.y + (slotWidth - parent.rect.width) * 0.5f : Rect.anchoredPosition.x,
            posInfo.z > 0 ? slotHeight * posInfo.w + (slotHeight - parent.rect.height) * 0.5f : Rect.anchoredPosition.y
            );
    }

    public void Clear() => image.DOFade(0f, fadeDuration).onComplete = Release;

    private void SetSprite(Sprite sprite, bool fade)
    {
        image.sprite = sprite;
        image.SetNativeSize();

        if (fade) image.DOFade(1f, fadeDuration);
    }
}
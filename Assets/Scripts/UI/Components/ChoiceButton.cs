using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChoiceButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Change Image")]
    [SerializeField] private Image image;
    [SerializeField] private Sprite enterSprite;

    private Sprite defaultSprite;

    [Header("Change Scale")]
    [SerializeField] private RectTransform rect;
    [SerializeField] private Vector3 enterScale = Vector3.one;

    private Vector3 defaultScale;

    private void OnValidate()
    {
        if (image != null) defaultSprite = image.sprite;
        if (rect != null) defaultScale = rect.localScale;
    }

    private void Start()
    {
        OnValidate();
        OnPointerExit(null);
    }

    public void OnPointerEnter(PointerEventData data)
    {
        if (image != null) image.sprite = enterSprite;
        if (rect != null) rect.localScale = enterScale;
    }
    
    public void OnPointerExit(PointerEventData data)
    {
        if (image != null) image.sprite = defaultSprite;
        if (rect != null) rect.localScale = defaultScale;
    }
}

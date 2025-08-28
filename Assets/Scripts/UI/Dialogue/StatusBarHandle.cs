using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StatusBarHandle : MonoBehaviour
{
    [SerializeField] private string id;
    public string ID => id;

    [Header("Components")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image image;
    [SerializeField] private Image Icon;

    [Header("Icons")]
    [SerializeField] private Sprite idleIcon;
    [SerializeField] private Sprite[] upIcons;
    [SerializeField] private Sprite[] downIcons;

    private bool animate;

    private float currentValue;
    private float changeValue;

    private float maxHP;

    public BindData<float> hp = new();

    private void Start()
    {
        currentValue = 1;
        changeValue = 1;

        image.material = Instantiate(image.material);
        Icon.sprite = idleIcon;

        PlayerManager.Instance.AddStatusBar(this);
    }

    public void SetActive(bool active) => canvasGroup.alpha = active ? 1 : 0;

    public void SetMaxHP(float value)
    {
        if (value == -1) return;

        maxHP = value;

        hp.SetValueOnly(value);

        hp.SetCallback(ChangeHP, SetCallbackType.Remove);
        hp.SetCallback(ChangeHP, SetCallbackType.Add);
    }

    public void OnHit(float value) => hp.Value -= value;

    private void ChangeHP(float newValue)
    {
        float value = changeValue * maxHP - newValue;

        if (value == 0) return;

        if (value > 0) Icon.sprite = downIcons[value < 2 ? 0 : 1];
        else Icon.sprite = upIcons[value > -2 ? 0 : 1];

        changeValue = newValue / maxHP;

        if (!animate)
        {
            animate = true;

            StartCoroutine(HPAnim());
        }
    }

    private IEnumerator HPAnim()
    {
        while (true)
        {
            currentValue = Mathf.Lerp(currentValue, changeValue, Time.deltaTime * 2);

            image.material.SetFloat("_FillAmount", currentValue);

            if (Mathf.Abs(currentValue - changeValue) < 0.001f) break;

            yield return null;
        }

        currentValue = changeValue;

        image.material.SetFloat("_FillAmount", currentValue);
        Icon.sprite = idleIcon;

        animate = false;
    }
}

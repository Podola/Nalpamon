using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NoteCharSlot : MonoBehaviour
{
    [SerializeField] private Button button;

    [SerializeField] private Image slot;
    [SerializeField] private Image icon;

    private NoteCharInfoSO info;
    public NoteCharInfoSO Info
    {
        get => info;
        set
        {
            if (value == null) icon.enabled = false;
            else
            {
                icon.enabled = true;
                icon.sprite = value.icon;
            }

            info = value;
        }
    }

    public void SetActive(bool active, Sprite sprite)
    {
        slot.sprite = sprite;

        icon.color = new Color32(255, 255, 255, (byte)(active ? 255 : 64));
    }

    public void SetEvent(int index, UnityAction<int> action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => action(index));
    }
}

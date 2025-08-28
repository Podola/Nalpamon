using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Localization.Components;

public class NoteClueSlot : MonoBehaviour
{
    [SerializeField] private Button button;

    [SerializeField] private Image icon;
    [SerializeField] private LocalizeSpriteEvent spriteEvent;
    [SerializeField] private Material outline;

    public bool Owned { get; set; }

    public bool IsSelected { get; private set; }

    private NoteClueInfoSO info;
    public NoteClueInfoSO Info
    {
        get => info;
        set
        {
            if (value == null) icon.color = Color.clear;
            else
            {
                icon.color = Color.white;

                spriteEvent.SetTable(value.AssetTable, value.icon.Value);
            }

            info = value;

            SetActive(IsSelected);
        }
    }

    private void Start() => icon.material = null;

    public void SetActive(bool active)
    {
        IsSelected = active;

        if (Owned) SetActiveOwned();
        else SetActiveUnowned();

        if (active) icon.material = outline;
        else icon.material = null;
    }

    private void SetActiveOwned() => icon.color = new Color32(255, 255, 255, (byte)(IsSelected ? 255 : 64));

    private void SetActiveUnowned() => icon.color = new Color32(0, 0, 0, (byte)(IsSelected ? 150 : 64));

    public void SetEvent(int index, UnityAction<int> action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => action(index));
    }
}

using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;

/// <summary>
/// NoteClueManager -> NoteCluePage로 이름 변경했습니다.
/// </summary>
public class NoteCluePage : NoteBase
{
    [SerializeField] private StringTableCollection table;
    [SerializeField] private LocalizeStringKey title;

    [SerializeField] private LocalizeStringEvent infoName;
    [SerializeField] private LocalizeStringEvent detail;

    [SerializeField] private Image icon;
    [SerializeField] private LocalizeSpriteEvent spriteEvent;
    [SerializeField] private Button select;

    [Header("Slot Data")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private NoteClueSlot[] slots;

    [SerializeField] private int scrollPower;


    private readonly ObjectVariable so = new();

    public string CurrentSlotID => slots[currentInfoIndex].Info.ID;

    public override void Init()
    {

        for (int i = 0; i < slots.Length; i++)
        {
            EnactiveSlot(i);

            slots[i].SetEvent(i, ChangeData);
        }
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);

        if (active)
        {
            SetSlot();
            ChangeData(currentInfoIndex);
        }
    }

    public override void ChangeData(int index)
    {
        if (slots[index].Info == null) return;

        EnactiveSlot(currentInfoIndex);

        currentInfoIndex = index;

        ActiveSlot(currentInfoIndex);

        SetData();
    }

    #region 슬롯 이동
    public void NextSlot()
    {
        if (currentInfoIndex == ChapterDataManager.Instance.NoteClue.Length - 1) return;

        ChangeData(currentInfoIndex + 1);
    }

    public void PrevSlot()
    {
        if (currentInfoIndex == 0) return;

        ChangeData(currentInfoIndex - 1);
    }

    public void UpSlots() => scrollRect.velocity += Vector2.up * scrollPower;

    public void DownSlots() => scrollRect.velocity += Vector2.down * scrollPower;
    #endregion

    public void SelectClue()
    {
        NoteController.Instance.Close();
    }

    private void SetData()
    {
        NoteClueInfoSO currentInfo = slots[currentInfoIndex].Info;

        detail.SetObjectVariable("so", so, currentInfo);

        if (slots[currentInfoIndex].Owned)
        {
            infoName.SetTable(currentInfo.Table, currentInfo.title);
            detail.SetTable(currentInfo.Table, currentInfo.KeyDict[0].value);

            icon.color = Color.white;

        }
        else
        {
            infoName.SetTable(table.TableCollectionName, title);
            detail.SetTable(currentInfo.Table, currentInfo.unowned);

            icon.color = new Color32(0, 0, 0, 150);

            select.gameObject.SetActive(false);
        }

        spriteEvent.SetTable(currentInfo.AssetTable, currentInfo.icon.Value);
    }

    private void SetSlot()
    {
        int slotIndex = 0;

        ChapterDataManager dataManager = ChapterDataManager.Instance;
        StepManager stepManager = StepManager.Instance;

        foreach (NoteClueInfoSO info in dataManager.NoteClue)
        {
            if (stepManager.GetMark(info.ID))
            {
                slots[slotIndex].Owned = true;
                slots[slotIndex].Info = info;

                slotIndex++;
            }
        }

        foreach (NoteClueInfoSO info in dataManager.NoteClue)
        {
            if (!stepManager.GetMark(info.ID))
            {
                slots[slotIndex].Owned = false;
                slots[slotIndex].Info = info;

                slotIndex++;
            }
        }

        for (; slotIndex < slots.Length; slotIndex++)
        {
            slots[slotIndex].Owned = false;
            slots[slotIndex].Info = null;
        }
    }

    private void ActiveSlot(int index) => slots[index].SetActive(true);

    private void EnactiveSlot(int index) => slots[index].SetActive(false);
}

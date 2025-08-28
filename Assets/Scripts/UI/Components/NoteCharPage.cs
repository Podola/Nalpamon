using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

/// <summary>
/// NoteCharManager -> NoteCharPage로 이름 변경했습니다.
/// </summary>
public class NoteCharPage : NoteBase
{
    [SerializeField] private Sprite[] slotSprite;
    [SerializeField] private NoteCharSlot[] slots;

    [SerializeField] private LocalizeStringEvent infoName;
    [SerializeField] private LocalizeStringEvent detail;
    [SerializeField] private LocalizeStringEvent speech;

    [SerializeField] private Image face;

    private int currentSlotPage;

    private readonly List<NoteCharInfoSO> charInfos = new();

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
            ChapterDataManager dataManager = ChapterDataManager.Instance;
            StepManager stepManager = StepManager.Instance;

            charInfos.Clear();

            foreach (var data in dataManager.NoteCharDict)
            {
                if (stepManager.GetMark(data.Key)) charInfos.Add(data.Value);
            }

            SetSlot();
            ChangeData(currentInfoIndex);
        }
    }

    public override void ChangeData(int slotIndex)
    {
        if (slots[slotIndex].Info == null) return;

        EnactiveSlot(currentInfoIndex);

        currentInfoIndex = slotIndex;

        ActiveSlot(currentInfoIndex);

        SetData();
    }

    public void NextSlots()
    {
        if (currentSlotPage == (charInfos.Count - 1) / slots.Length) return;

        currentSlotPage++;

        SetSlot();
        ChangeData(0);
    }

    public void PrevSlots()
    {
        if (currentSlotPage == 0) return;

        currentSlotPage--;

        SetSlot();
        ChangeData(0);
    }

    private void SetData()
    {
        NoteCharInfoSO currentInfo = slots[currentInfoIndex].Info;

        infoName.SetTable(currentInfo.Table, currentInfo.title);
        detail.SetTable(currentInfo.Table, currentInfo.detail);
        speech.SetTable(currentInfo.Table, currentInfo.speech);

        face.sprite = currentInfo.face;
    }

    private void SetSlot()
    {
        int infoIndex = currentSlotPage * slots.Length;

        for (int i = 0; i < slots.Length; i++, infoIndex++)
        {
            if (charInfos.Count > infoIndex) slots[i].Info = charInfos[infoIndex];
            else slots[i].Info = null;
        }
    }

    private void ActiveSlot(int index) => slots[index].SetActive(true, slotSprite[1]);

    private void EnactiveSlot(int index) => slots[index].SetActive(false, slotSprite[0]);
}

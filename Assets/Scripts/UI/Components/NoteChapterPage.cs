using UnityEngine;
using UnityEngine.Localization.Components;

/// <summary>
/// NoteChapterManager -> NoteChapterPage로 이름 변경했습니다.
/// </summary>
public class NoteChapterPage : NoteBase
{
    [SerializeField] private LocalizeStringEvent infoName;
    [SerializeField] private LocalizeStringEvent detail;

    private NoteChapterInfoSO[] infos;

    public int CurrentInfoIndex => currentInfoIndex + 1;

    public override void Init() => infos = ChapterDataManager.Instance.NoteChapter;

    public override void SetActive(bool active)
    {
        base.SetActive(active);

        if (active) SetData();
    }

    public override void ChangeData(int index) => SetData();

    private void SetData()
    {
        NoteChapterInfoSO info = infos[currentInfoIndex];

        infoName.SetTable(info.Table, info.title);
        detail.SetTable(info.Table, info.detail);
    }

    public void SetInfos(NoteChapterInfoSO[] infos) => this.infos = infos;
}

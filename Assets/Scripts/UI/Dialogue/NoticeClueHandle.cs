using UnityEngine;
using UnityEngine.Localization.Components;

public class NoticeClueHandle : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private LocalizeSpriteEvent icon;

    private void Awake()
    {
        StepManager stepManager = StepManager.Instance;
    }

    private void OnEnable()
    {
        // GameEvents 구독
        GameEvents.OnMarkChanged += Notice;
    }

    private void OnDisable()
    {
        // GameEvents 구독 해제
        GameEvents.OnMarkChanged -= Notice;
    }

    private void Notice(string id, bool value)
    {
        // value가 true일 때만 (마크가 생성될 때만) 알림
        if (!value) return;

        ChapterDataManager dataManager = ChapterDataManager.Instance;

        if (dataManager.NoteClueDict.ContainsKey(id))
        {
            NoteClueInfoSO info = dataManager.NoteClueDict[id];
            icon.SetTable(info.AssetTable, info.icon.Value);
            animator.Play("Notice");
        }
    }
}

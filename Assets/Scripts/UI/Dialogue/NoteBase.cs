using UnityEngine;

public abstract class NoteBase : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private NoteBookmarkHandle bookmarkHandle;

    protected int currentInfoIndex;

    private void Start()
    {
        SetActive(false);
        Init();
    }

    public abstract void Init();

    public abstract void ChangeData(int index);

    public virtual void SetActive(bool active)
    {
        bookmarkHandle.HasOpen = active;
        bookmarkHandle.Highlight(active);

        animator.Play(active ? "Open" : "Close");
    }
}

using System.Collections;
using UnityEngine;

/// <summary>
/// 게임 내 수첩(노트) UI의 열기, 닫기, 페이지 전환 등 전체적인 동작을 관리합니다.
/// </summary>
public class NoteController : Singleton<NoteController>
{
    [SerializeField] private Animator animator;
    [SerializeField] private NoteBase[] pages;

    private int currentPage = 0;
    private bool isOpen = false;

    /// <summary>
    /// 지정된 페이지로 전환합니다.
    /// </summary>
    public void ChangePage(int index)
    {
        if (currentPage == index || index < 0 || index >= pages.Length) return;

        pages[currentPage].SetActive(false);
        currentPage = index;
        pages[currentPage].SetActive(true);
    }

    /// <summary>
    /// 수첩 UI를 엽니다.
    /// </summary>
    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        InputManager.Instance.SetPopupActive(true);
        gameObject.SetActive(true);
        animator.Play("Open");

        StartCoroutine(WaitForOpenAnimation());
    }

    /// <summary>
    /// 수첩 UI를 닫습니다.
    /// </summary>
    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        InputManager.Instance.SetPopupActive(false);
        animator.Play("Close");
        pages[currentPage].SetActive(false);

        // 애니메이션 종료 후 비활성화 (선택적)
        // StartCoroutine(WaitForCloseAnimation());
    }

    private IEnumerator WaitForOpenAnimation()
    {
        // 애니메이션이 끝날 때까지 기다리거나, 일정 시간 후 페이지를 엽니다.
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Active"));
        pages[currentPage].SetActive(true);
    }
}
using System.Collections;
using UnityEngine;

public class BubbleLineTarget : MonoBehaviour
{
    [SerializeField] private string id;
    public string ID => id;

    private Animator animator;

    private BubbleLineHandle handle;
    public BubbleLineHandle Handle
    {
        get => handle;
        set
        {
            handle = value;

            if (handle != null) handle.SetTarget(this);
        }
    }

    private void Start()
    {
        BubbleLinePresenter presenter = FindAnyObjectByType<BubbleLinePresenter>();

        presenter.AddTarget(this);

        animator = GetComponent<Animator>();
    }

    public void PlayAnimayion(string name) => animator.Play(name);
}

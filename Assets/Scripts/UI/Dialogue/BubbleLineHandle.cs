using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class BubbleLineHandle : MonoBehaviour
{
    [SerializeField] private TMP_Text bubbleText;
    public TMP_Text BubbleText => bubbleText;

    private Camera cam;

    private BubbleLineTarget target;

    private IObjectPool<BubbleLineHandle> pool;

    private void Update()
    {
        transform.position = cam.WorldToScreenPoint(target.transform.position);
    }

    public void SetPool(IObjectPool<BubbleLineHandle> pool)
    {
        this.pool = pool;

        cam = Camera.main;
    }

    public void Release()
    {
        pool.Release(this);

        target.Handle = null;
        target = null;
    }

    public void SetActive(bool active) => gameObject.SetActive(active);

    public void SetTarget(BubbleLineTarget target) => this.target = target;
}

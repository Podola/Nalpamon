using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class NoteBookmarkHandle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float speed = 10;
    [SerializeField] private Vector2 highlightPos;

    private bool isMove = false;

    private Vector2 movePos;
    private Vector2 originPos;

    public bool HasOpen { get; set; }

    private void Awake() => originPos = transform.localPosition;

    private void OnDisable() => isMove = false;

    public void OnPointerEnter(PointerEventData data) => Highlight(true);

    public void OnPointerExit(PointerEventData data) => Highlight(false);

    public void Highlight(bool highlight)
    {
        if (!highlight && HasOpen) return;

        movePos = originPos + (highlight ? highlightPos : Vector2.zero);

        if (!isMove) StartCoroutine(Highlight());
    }

    private IEnumerator Highlight()
    {
        isMove = true;

        while (true)
        {
            transform.localPosition = Vector2.Lerp(transform.localPosition, movePos, speed * Time.deltaTime);

            if (Vector2.Distance(transform.localPosition, movePos) < 0.01f) break;

            yield return null;
        }

        isMove = false;
    }
}

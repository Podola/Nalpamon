using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class SimpleTrigger : MonoBehaviour
{
    public bool OneTime = true;
    public UnityEvent OnEnter;
    public UnityEvent OnExit;

    public bool Triggered = false;
    private string _tag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (OneTime && Triggered) return;
        if (!other.CompareTag(_tag)) return;

        Log.V($"[SimpleTrigger] '{gameObject.name}' 트리거 진입 감지됨");
        OnEnter?.Invoke();
        Triggered = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(_tag)) return;

        OnExit?.Invoke();
    }
}
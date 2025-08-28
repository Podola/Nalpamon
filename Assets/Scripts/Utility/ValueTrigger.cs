using System;
using UnityEngine;

[Serializable]
public class ValueTrigger<T>
{
    [SerializeField] private string[] triggers;

    [SerializeField] private T normal;
    [SerializeField] private T trigger;

    public T Value
    {
        get
        {
            if (triggers.Length == 0) return normal;

            foreach (string trigger in triggers)
            {
                if (!StepManager.Instance.GetMark(trigger)) return normal;
            }

            return trigger;
        }
    }

    public static implicit operator T(ValueTrigger<T> valueTrigger) => valueTrigger.Value;
}

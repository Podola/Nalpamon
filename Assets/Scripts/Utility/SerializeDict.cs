using System;
using UnityEngine;

[Serializable]
public class SerializeDict<T, U>
{
    [SerializeField] private SerializePair<T, U>[] serializePair;

    public int Count => serializePair.Length;

    public SerializeDict(int length)
    {
        serializePair = new SerializePair<T, U>[length];

        for (int i = 0; i < length; i++) serializePair[i] = new();
    }

    public SerializePair<T, U> this[int index] => serializePair[index];
}

[Serializable]
public class SerializePair<T, U>
{
    public T key;
    public U value;
}

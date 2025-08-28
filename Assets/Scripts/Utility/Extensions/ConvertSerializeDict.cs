using System.Collections.Generic;

public static class ConvertSerializeDict
{
    public static Dictionary<T, U> Convert<T, U>(this SerializeDict<T, U> infos)
    {
        Dictionary<T, U> dict = new();

        for (int i = 0; i < infos.Count; i++) dict.Add(infos[i].key, infos[i].value);

        return dict;
    }

    public static SerializeDict<T, U> Convert<T, U>(this Dictionary<T, U> infos)
    {
        SerializeDict<T, U> dict = new(infos.Count);

        int index = 0;

        foreach (KeyValuePair<T, U> info in infos)
        {
            dict[index].key = info.Key;
            dict[index].value = info.Value;

            index++;
        }

        return dict;
    }
}

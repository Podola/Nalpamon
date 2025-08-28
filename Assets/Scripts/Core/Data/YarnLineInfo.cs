using System.Collections.Generic;

public struct YarnLineInfo
{
    public string LineID { get; private set; }

    public string[] Metadata { get; private set; }

    public readonly string Name => Pair.Key;

    public readonly string Text => Pair.Value;

    public KeyValuePair<string, string> Pair { get; private set; }

    public YarnLineInfo(string lineID, string[] metadata, string name, string text)
    {
        LineID = lineID;

        Metadata = metadata ?? (new string[0]);

        Pair = new(name, text);
    }
}
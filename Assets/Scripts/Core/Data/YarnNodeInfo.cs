using System.Collections.Generic;
using Yarn.Unity;

public struct YarnNodeInfo
{
    public string NodeName { get; private set; }

    private readonly List<string> lineIDs;

    private readonly Dictionary<string, List<string>> charInfos;

    private readonly Dictionary<string, YarnLineInfo> lineInfos;

    public readonly int LineCount => lineIDs.Count;

    public readonly int CharacterCount => charInfos.Count;

    public YarnNodeInfo(string nodeName, Localization localization, LineMetadata metadata, Yarn.Program program)
    {
        NodeName = nodeName;

        lineIDs = new();
        lineInfos = new();

        charInfos = new();

        foreach (string lineID in program.LineIDsForNode(nodeName))
        {
            string[] splitLine = localization.GetLocalizedString(lineID).Split(':');

            YarnLineInfo lineInfo = new(
                lineID,
                metadata.GetMetadata(lineID),
                splitLine.Length == 1 ? "???" : splitLine[0].Trim(),
                splitLine[^1].Trim()
            );

            lineIDs.Add(lineID);
            lineInfos.Add(lineID, lineInfo);

            if (charInfos.ContainsKey(lineInfo.Name))
            {
                if (charInfos[lineInfo.Name] == null) charInfos[lineInfo.Name] = new() { lineID };
                else charInfos[lineInfo.Name].Add(lineID);
            }
            else charInfos.Add(lineInfo.Name, new() { lineID });
        }
    }

    public readonly IEnumerable<string> GetNames()
    {
        foreach (string lineID in lineIDs) yield return lineInfos[lineID].Name;
    }

    public readonly IEnumerable<string> GetTexts()
    {
        foreach (string lineID in lineIDs) yield return lineInfos[lineID].Text;
    }

    public readonly IEnumerable<KeyValuePair<string, string>> GetLines()
    {
        foreach (string lineID in lineIDs) yield return lineInfos[lineID].Pair;
    }

    public readonly int GetNextLineIndex(string lineID)
    {
        int lineIndex = lineIDs.IndexOf(lineID) + 1;

        return lineIndex < lineIDs.Count ? lineIndex : -1;
    }

    #region Metadata
    public readonly string[] GetMetadata(string lineID) => lineInfos[lineID].Metadata;

    public readonly string GetTag(int index, string contains) => GetTag(lineIDs[index], contains);

    public readonly string GetTag(string lineID, string contains)
    {
        string result = "";

        foreach (string metadata in GetMetadata(lineID))
        {
            if (metadata.StartsWith(contains))
            {
                result = metadata[contains.Length..];

                break;
            }
        }

        return result;
    }
    #endregion

    #region Name To Data
    public readonly IEnumerable<string> GetCharNames()
    {
        foreach (string character in charInfos.Keys) yield return character;
    }

    public readonly IEnumerable<KeyValuePair<string, string>> GetCharLines(string charName)
    {
        foreach (string lineID in charInfos[charName]) yield return lineInfos[lineID].Pair;
    }

    public readonly int GetLineCount(string charName) => charInfos[charName].Count;

    public readonly bool Contains(string charName) => charInfos.ContainsKey(charName);
    #endregion

    #region Index To Data
    public readonly string GetName(int index) => lineInfos[lineIDs[index]].Name;

    public readonly string GetText(int index) => lineInfos[lineIDs[index]].Text;

    public readonly KeyValuePair<string, string> GetLine(int index)
    {
        string lineID = lineIDs[index];

        return lineInfos[lineID].Pair;
    }

    public readonly string[] GetMetadata(int index) => lineInfos[lineIDs[index]].Metadata;
    #endregion
}
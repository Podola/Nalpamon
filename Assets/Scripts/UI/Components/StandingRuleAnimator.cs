using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class StandingRuleAnimator
{
    private YarnNodeInfo nodeInfo;

    private string firstCharName = "";

    private string secondCharName = "";

    private string standingRuleType;

    private string currentLineID;

    private readonly float duration = 0.2f;

    private readonly DialogueRunner runner;

    private readonly StandingDisplay standingDisplay;

    private readonly HashSet<string> mainCharNames = new();

    public StandingRuleAnimator(DialogueRunner runner, StandingDisplay standingDisplay)
    {
        this.runner = runner;
        this.standingDisplay = standingDisplay;

        standingRuleType = "Normal";

        foreach (CharInfoSO info in standingDisplay.MainChars) mainCharNames.Add(info.ID);
    }

    public void Clear()
    {
        firstCharName = "";
        secondCharName = "";
    }

    public void OnNodeStart(string nodeName)
    {
        nodeInfo = runner.GetNodeInfo(nodeName);

        int mainCharCount = 0;

        foreach (string charName in nodeInfo.GetCharNames())
        {
            if (mainCharNames.Contains(charName)) mainCharCount++;
        }

        if (nodeInfo.CharacterCount == mainCharCount)
        {
            Debug.Log("주요 인물만 등장");
        }
        else if (mainCharCount == 0)
        {
            Debug.Log($"주요 인물 미등장, 그 외 {nodeInfo.CharacterCount - mainCharCount}명 등장");
        }
        else
        {
            Debug.Log($"주요 인물 외 {nodeInfo.CharacterCount - mainCharCount}명 등장");
        }
    }

    public void OnLineEvent(LocalizedLine line)
    {
        currentLineID = line.TextID;

        string standingID = GetStandingID(currentLineID);

        switch (standingRuleType)
        {
            case "Normal":
                NormalStanding(line.CharacterName, standingID);
                break;

            case "Rebuttal":
                RebuttalStanding(line.CharacterName, standingID);
                break;

            case "Interro":
                InterroStanding(line.CharacterName, standingID);
                break;

            default:
                standingRuleType = "Normal";

                NormalStanding(line.CharacterName, standingID);
                break;
        }
    }

    private void NormalStanding(string charName, string standingID)
    {
        string leftPos = "5/1";
        string rightPos = "5/3";

        if (firstCharName == "")
        {
            if (mainCharNames.Contains(charName)) standingDisplay.LeftIn(charName, standingID, true, duration, leftPos);
            else standingDisplay.RightIn(charName, standingID, false, duration, rightPos);
        }
        else
        {
            if (charName == firstCharName) standingDisplay.ChangeStanding(charName, standingID);
            else
            {
                CharStandingHandle handle = standingDisplay.GetHandle(charName);

                if (handle.IsRight) standingDisplay.LeftOut(charName, standingID, true, duration * 0.5f);
                else standingDisplay.RightOut(charName, standingID, false, duration * 0.5f);

                if (mainCharNames.Contains(charName)) standingDisplay.LeftIn(charName, standingID, true, duration * 0.5f, leftPos);
                else standingDisplay.RightIn(charName, standingID, false, duration * 0.5f, rightPos);
            }
        }

        firstCharName = charName;
    }

    private void RebuttalStanding(string charName, string standingID)
    {
        string pos = "center";

        if (firstCharName == "") standingDisplay.RightIn(charName, standingID, false, duration, pos);
        else standingDisplay.ShowOrMove(charName, standingID, false, duration, pos);

        standingDisplay.FixSiblingIndex(charName, 1);

        firstCharName = charName;
    }

    private void InterroStanding(string charName, string standingID)
    {
        string leftPos = "5/1";
        string leftYPos = "1/0";

        string rightPos = "5/3";

        string backStandingID = "Back";
        string mainCharName = standingDisplay.MainChars[0].ID;

        if (firstCharName == "" && secondCharName == "")
        {
            firstCharName = mainCharName;
            secondCharName = charName;

            standingDisplay.LeftIn(firstCharName, backStandingID, true, duration, leftPos, leftYPos);
            standingDisplay.RightIn(secondCharName, standingID, false, duration, rightPos);
        }
        else
        {
            if (charName == secondCharName) standingDisplay.ChangeStanding(charName, standingID);
            else if (charName == firstCharName)
            {
                standingDisplay.ChangePosition(charName, standingID, secondCharName, backStandingID, duration);

                firstCharName = secondCharName;
                secondCharName = charName;
            }
            else
            {
                if (mainCharName == secondCharName)
                {
                    standingDisplay.RightOutRightIn(mainCharName, backStandingID, true, duration, leftPos, leftYPos);

                    standingDisplay.LeftOut(firstCharName, "", true, duration * 0.5f);
                    standingDisplay.LeftIn(charName, standingID, false, duration * 0.5f, rightPos);

                    firstCharName = mainCharName;
                    secondCharName = charName;
                }
                else
                {
                    standingDisplay.RightOutNewLeftIn(secondCharName, "", charName, standingID, duration);

                    secondCharName = charName;
                }
            }
        }

        standingDisplay.FixSiblingIndex(firstCharName, 1);
        standingDisplay.FixSiblingIndex(secondCharName, 2);
    }

    public void ChangeRuleType(string ruleType, string charName = "", string standingID = "")
    {
        standingRuleType = ruleType;

        standingDisplay.ClearStandings();

        switch (standingRuleType)
        {
            case "Normal":
                break;

            case "Rebuttal":
                if (currentLineID != "")
                {
                    if (charName == "" && standingID == "")
                    {
                        int nextLineImdex = nodeInfo.GetNextLineIndex(currentLineID);

                        charName = nodeInfo.GetName(nextLineImdex);
                        standingID = GetStandingID(nextLineImdex);
                    }

                    RebuttalStanding(charName, standingID);
                }
                break;

            case "Interro":
                if (currentLineID != "")
                {
                    if (charName == "" && standingID == "")
                    {
                        int nextLineImdex = nodeInfo.GetNextLineIndex(currentLineID);

                        charName = nodeInfo.GetName(nextLineImdex);
                        standingID = GetStandingID(nextLineImdex);
                    }

                    InterroStanding(charName, standingID);
                }
                break;

            default:
                standingRuleType = "Normal";
                break;
        }
    }

    private string GetStandingID(string lineID)
    {
        string standingID = nodeInfo.GetTag(lineID, "face:");

        if (standingID == "") standingID = "01-01";

        return standingID;
    }

    private string GetStandingID(int lineIndex)
    {
        string standingID = nodeInfo.GetTag(lineIndex, "face:");

        if (standingID == "") standingID = "01-01";

        return standingID;
    }
}
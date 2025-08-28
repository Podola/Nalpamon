using System.Linq;
using UnityEngine;

public static class Util
{
    public static bool NodeExists(string nodeName, bool warnIfMissing = true)
    {
        var runner = DialogueManager.Instance != null ? DialogueManager.Instance.Runner : null;
        var project = runner != null ? runner.YarnProject : null;

        if (project == null)
        {
            if (warnIfMissing)
                Log.W("DialogueRunner or YarnProject not found");
            return false;
        }

        bool exists = project.NodeNames.Contains(nodeName);
        if (!exists && warnIfMissing)
            Log.W($"Node not found: {nodeName}");
        return exists;
    }

    public static string ToString(StepId step) => step.ToString();

    public static string StepEntryNode(StepId step) => $"Step_{ToString(step)}";
    public static string RoomEntryNode(RoomType room, StepId step) => $"Room_{room}_Enter_{ToString(step)}";
    public static string InteractNode(string baseName, StepId step) => $"{baseName}_{ToString(step)}";

    public static string Mark(MarkType type, string name) => $"Mark_{type}_{name}";
    public static string Mark(MarkType type, string name, StepId step) => $"Mark_{type}_{name}_{ToString(step)}";

    public static string StepEntryKey(StepId step) => $"Mark_SYS_StepEntry_{ToString(step)}";

    public static string RoomVisitedGlobal(RoomType room) => Mark(MarkType.ROOM_VISITED, room.ToString());
    public static string RoomVisitedInStep(RoomType room, StepId step) => Mark(MarkType.ROOM_VISITED, room.ToString(), step);

    public static string MarkDaily(MarkType type, string name) => $"Mark_DAILY_{type}_{name}";

    public static string DayOf(StepId step)
    {
        var s = step.ToString();
        var p = s.Split('_');
        return (p.Length >= 2) ? (p[0] + "_" + p[1]) : s;
    }

    public static string GateAutoName(StepId step) => "StepGate";
    public static string MarkGateSucceeded(StepId step) => Mark(MarkType.GATE_SUCCEEDED, GateAutoName(step), step);
    public static string GateSuccessNode(StepId step) => $"Gate_{ToString(step)}_Success";
    public static string GateFailNode(StepId step) => $"Gate_{ToString(step)}_Fail";

    public static string NpcStepFirstKey(string npc, StepId step)
        => $"Mark_SYS_NPCStepFirst_{npc}_{ToString(step)}";

    public static bool HasStepRepeat(string baseName, StepId step)
        => NodeExists($"{baseName}_{step}_Repeat", false);

    public static bool HasRepeat(string baseName)
        => NodeExists($"{baseName}_Repeat", false);

    public static bool HasRoomRepeat(RoomType room, string baseName)
        => NodeExists($"Room_{room}_{baseName}_Repeat", false);

    public static bool HasAnyDailyNode(string baseName, StepId step)
    {
        string stepStr = ToString(step);
        string dayStr = DayOf(step);

        string[] candidates =
        {
            $"{baseName}_{stepStr}_First",
            $"{baseName}_{dayStr}_First",
            $"{baseName}_First",
            $"{baseName}_{stepStr}_Repeat",
            $"{baseName}_{dayStr}_Repeat",
            $"{baseName}_Repeat",
        };

        for (int i = 0; i < candidates.Length; i++)
            if (NodeExists(candidates[i], false))
                return true;

        return false;
    }

    /// <summary>
    /// StepId 열거형의 이름에서 챕터 번호를 추출합니다. (예: C1_D1_10 -> 1)
    /// </summary>
    public static int GetChapterFromStepId(StepId stepId)
    {
        string stepName = stepId.ToString(); // "C1_D1_10"
        if (stepName.StartsWith("C"))
        {
            string chapterPart = stepName.Split('_')[0]; // "C1"
            string numberPart = chapterPart.Substring(1); // "1"
            if (int.TryParse(numberPart, out int chapterNumber))
            {
                return chapterNumber;
            }
        }
        return 0; // 챕터 0이거나 형식이 맞지 않으면 0을 반환
    }
}
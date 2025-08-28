using System.Collections.Generic;
using Yarn.Unity;

public static class DialogueExtension
{
    public static void AddPresenter(this DialogueRunner runner, DialoguePresenterBase presenter)
    {
        List<DialoguePresenterBase> presenters = new(runner.DialoguePresenters);

        if (presenters.Contains(presenter)) return;

        presenters.Add(presenter);

        runner.DialoguePresenters = presenters;
    }

    public static void RemovePresenter(this DialogueRunner runner, DialoguePresenterBase presenter)
    {
        List<DialoguePresenterBase> presenters = new(runner.DialoguePresenters);

        presenters.Remove(presenter);

        runner.DialoguePresenters = presenters;
    }

    public static string GetNodeTag(this DialogueRunner runner, int index)
    {
        string tags = runner.Dialogue.GetHeaderValue(runner.Dialogue.CurrentNode, "tags");

        if (string.IsNullOrEmpty(tags)) return string.Empty;

        string[] splitTags = tags.Split(' ');

        if (splitTags.Length <= index) return string.Empty;
        else return splitTags[index];
    }

    public static string GetLineTag(this LocalizedLine line, string tag)
    {
        foreach (string tagValue in line.Metadata)
        {
            if (tagValue.StartsWith(tag)) return tagValue[tag.Length..];
        }

        return "";
    }

    public static YarnNodeInfo GetNodeInfo(this DialogueRunner runner, string nodeName)
    {
        Localization localization = runner.YarnProject.GetLocalization(runner.LineProvider.LocaleCode);
        LineMetadata metadata = runner.YarnProject.lineMetadata;

        return new(nodeName, localization, metadata, runner.YarnProject.Program);
    }
}
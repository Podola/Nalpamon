using UnityEngine;
using Yarn.Unity;

public class Teeest : MonoBehaviour
{
    public YarnProject yarnProject;

    private DialogueRunner runner;

    private void Start()
    {
        GameManager.Instance.SetupChapter(0);

        runner = FindFirstObjectByType<DialogueRunner>();
    }

    public void SetEvent() => runner.SetProject(yarnProject);

    public void Test(string node) => runner.StartDialogue(node);
}

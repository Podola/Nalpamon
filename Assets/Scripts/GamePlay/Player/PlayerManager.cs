using System.Collections.Generic;
using Yarn.Unity;

public class PlayerManager : Singleton<PlayerManager>
{
    private readonly Dictionary<string, StatusBarHandle> statusBarHandles = new();

    private void Start()
    {
        StepManager stepManager = StepManager.Instance;

        stepManager.SetMark("Detective", true);
        stepManager.SetMark("Jyoshyu", true);
        stepManager.SetMark("Azathoth", true);

        stepManager.SetMark("Broken_Window", true);

        DialogueRunner runner = FindFirstObjectByType<DialogueRunner>();

        runner.AddCommandHandler<string, bool, float>("SetActiveStatusBar", SetActiveStatusBar);
        runner.AddCommandHandler<string, float>("Attack", OnAttack);
    }

    #region Status
    public void AddStatusBar(StatusBarHandle statusBarHandle)
    {
        if (statusBarHandles.ContainsKey(statusBarHandle.ID)) return;

        statusBarHandles.Add(statusBarHandle.ID, statusBarHandle);
    }

    public void SetActiveStatusBar(string id, bool active, float value = -1)
    {
        if (!statusBarHandles.ContainsKey(id)) return;

        statusBarHandles[id].SetActive(active);
        statusBarHandles[id].SetMaxHP(value);
    }

    public void OnAttack(string id, float value)
    {
        if (statusBarHandles.ContainsKey(id)) statusBarHandles[id].OnHit(value);
    }
    #endregion
}

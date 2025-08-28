using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class TaskDisplay: MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentTaskText;

    public CanvasGroup _canvasGroup;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromGameEvents();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isChapterScene = FindFirstObjectByType<ChapterControllerBase>() != null;
        SetActiveState(isChapterScene);
    }

    private void SetActiveState(bool isActive)
    {
        _canvasGroup.alpha = isActive ? 1f : 0f;
        _canvasGroup.interactable = isActive;
        _canvasGroup.blocksRaycasts = isActive;

        if (isActive)
        {
            SubscribeToGameEvents();
            if (StepManager.Instance != null)
            {
                UpdateDisplay(StepManager.Instance.CurrentStep, StepManager.Instance.CurrentStep);
            }
        }
        else
        {
            UnsubscribeFromGameEvents();
        }
    }

    private void SubscribeToGameEvents()
    {
        UnsubscribeFromGameEvents();
        GameEvents.OnStepChanged += UpdateDisplay;
        GameEvents.OnMarkChanged += HandleMarkChanged;
    }

    private void UnsubscribeFromGameEvents()
    {
        GameEvents.OnStepChanged -= UpdateDisplay;
        GameEvents.OnMarkChanged -= HandleMarkChanged;
    }

    private void HandleMarkChanged(string key, bool value)
    {
        if (StepManager.Instance != null)
        {
            UpdateDisplay(StepManager.Instance.CurrentStep, StepManager.Instance.CurrentStep);
        }
    }

    private void UpdateDisplay(StepId prev, StepId current)
    {
        if (currentTaskText != null)
        {
            var sm = StepManager.Instance;
            if (sm == null) return;

            var config = sm.GetConfig(current);
            if (config != null && config.tasks.Any())
            {
                var pendingTask = config.tasks
                    .OrderBy(t => t.order)
                    .FirstOrDefault(t => !sm.GetMark(sm.GetTaskDoneKey(t, t.type, t.name)));

                if (pendingTask != null)
                {
                    currentTaskText.text = $"해야 할 일: {pendingTask.name} ({pendingTask.type})";
                }
                else
                {
                    currentTaskText.text = "모든 과업 완료!";
                }
            }
            else
            {
                currentTaskText.text = "자유롭게 탐험하세요.";
            }
        }
    }
}
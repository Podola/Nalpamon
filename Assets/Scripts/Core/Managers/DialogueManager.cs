using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Yarn Spinner의 DialogueRunner를 관리하고,
/// 대화 시작 및 종료 이벤트를 GameEvents를 통해 전파하는 중앙 허브입니다.
/// </summary>
public class DialogueManager : Singleton<DialogueManager>
{
    public DialogueRunner Runner;

    public LineAdvancer Advancer;
    /// <summary>
    /// 현재 대화가 진행 중인지 여부를 반환합니다.
    /// </summary>
    public bool IsRunning => Runner != null && Runner.IsDialogueRunning;

    protected override void Awake()
    {
        base.Awake(); // 싱글톤 초기화

        if (Runner == null)
            Runner = FindFirstObjectByType<DialogueRunner>();

        if (Runner == null)
        {
            Debug.LogError("[DialogueManager] DialogueRunner를 찾을 수 없습니다.");
            return;
        }

        // DialogueRunner의 이벤트에 리스너를 등록합니다.
        Runner.onDialogueStart.AddListener(HandleDialogueStart);
        Runner.onDialogueComplete.AddListener(HandleDialogueComplete);
    }

    private void HandleDialogueStart()
    {
        GameEvents.TriggerDialogueStarted(); // 대화 시작 이벤트를 전파합니다.
    }

    private void HandleDialogueComplete()
    {
        GameEvents.TriggerDialogueCompleted(); // 대화 종료 이벤트를 전파합니다.
    }

    /// <summary>
    /// 지정된 Yarn 노드에서 대화를 시작합니다.
    /// </summary>
    /// <param name="yarnNode">시작할 Yarn 노드의 이름</param>
    public static void StartDialogue(string yarnNode)
    {
        if (string.IsNullOrEmpty(yarnNode) || Instance == null || Instance.Runner == null)
        {
            Debug.LogWarning($"[DialogueManager] 대화를 시작할 수 없습니다. Node: {yarnNode}");
            return;
        }

        Instance.Runner.StartDialogue(yarnNode);
    }

    private void OnDestroy()
    {
        // 오브젝트 파괴 시 리스너를 안전하게 제거합니다.
        if (Runner != null)
        {
            Runner.onDialogueStart.RemoveListener(HandleDialogueStart);
            Runner.onDialogueComplete.RemoveListener(HandleDialogueComplete);
        }
    }
}
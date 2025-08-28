using UnityEngine;
using Yarn.Unity;
using System.Collections.Generic;

/// <summary>
/// 현재 씬의 모든 Dialogue Presenter들을 원하는 순서대로 DialogueRunner에 등록하고 해제합니다.
/// </summary>
public class ScenePresenterBinder : MonoBehaviour
{
    [Header("이 씬에서 등록할 Presenter 목록")]
    [Tooltip("이 리스트의 순서대로 DialogueRunner에 추가됩니다. (0번이 가장 먼저 실행됨)")]
    [SerializeField] private List<DialoguePresenterBase> presentersToBind;

    private DialogueRunner _dialogueRunner;

    void Start()
    {
        _dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        if (_dialogueRunner == null)
        {
            Debug.LogError("[ScenePresenterBinder] DialogueRunner를 찾을 수 없습니다!");
            return;
        }

        // 인스펙터에 설정된 순서 그대로 Presenter들을 등록합니다.
        foreach (var presenter in presentersToBind)
        {
            if (presenter != null)
            {
                _dialogueRunner.AddPresenter(presenter);
            }
        }

        _dialogueRunner.AddPresenter(DialogueManager.Instance.Advancer);
    }

    private void OnDestroy()
    {
        if (_dialogueRunner != null && presentersToBind != null)
        {
            foreach (var presenter in presentersToBind)
            {
                if (presenter != null)
                {
                    _dialogueRunner.RemovePresenter(presenter);
                }
            }
        }
    }
}
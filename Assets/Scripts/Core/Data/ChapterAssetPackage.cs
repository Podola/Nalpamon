using UnityEngine;
using Yarn.Unity;

[System.Serializable]
public class ChapterAssetPackage
{
    public string chapterName; // 인스펙터에서 구분을 위한 이름
    public string sceneName;   // 이 챕터에 해당하는 씬 이름 (예: "Chapter1Scene")
    public ChapterDataSO chapterData;
    public StepDatabaseSO stepDatabase;

    [Tooltip("이 챕터를 '새로 시작'할 때 사용할 초기 상태 데이터")]
    public ChapterStartStateSO startState;

    [Tooltip("이 챕터에서 사용할 Yarn Project")]
    public YarnProject yarnProject;
}
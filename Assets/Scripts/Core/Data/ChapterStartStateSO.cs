using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChapterStartState", menuName = "Game/Chapter Start State")]
public class ChapterStartStateSO : ScriptableObject
{
    [Header("시작 Step 정보")]
    [Tooltip("이 챕터를 시작할 때 진입할 첫 Step입니다.")]
    public StepId startStepId;

    [Header("시작 위치 정보")]
    public RoomType startRoom;
    public string startTargetPointKey;

    [Header("초기 Mark 설정")]
    [Tooltip("이 챕터 시작 시 자동으로 true로 설정할 Mark 목록")]
    public List<string> initialMarks;
}
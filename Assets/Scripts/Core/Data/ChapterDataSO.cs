using UnityEngine;

[CreateAssetMenu(fileName = "NewChapterData", menuName = "SO/ChapterData")]
public class ChapterDataSO : ScriptableObject
{
    public InterroPhaseInfoSO[] interroPhaseInfos;
    public EnvisionInfoSO[] envisionInfos;

    public CharInfoSO[] charInfos;

    public NoteChapterInfoSO[] noteChapterInfos;
    public NoteClueInfoSO[] noteClueInfos;
}

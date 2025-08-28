using UnityEditor.Localization;
using UnityEngine;
using Yang.Localize;

[CreateAssetMenu(fileName = "NewChapterInfoSO", menuName = "SO/Note/ChapterInfo")]
public class NoteChapterInfoSO : ScriptableObject
{
    [LocalizeTable, SerializeField] private StringTableCollection table;
    public string Table => table.TableCollectionName;

    public LocalizeStringKey title;
    public LocalizeStringKey detail;
}

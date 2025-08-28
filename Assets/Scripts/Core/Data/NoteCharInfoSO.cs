using UnityEditor.Localization;
using UnityEngine;
using Yang.Localize;

[CreateAssetMenu(fileName = "NewCharInfoSO", menuName = "SO/Note/CharInfo")]
public class NoteCharInfoSO : ScriptableObject
{
    [LocalizeTable, SerializeField] private StringTableCollection table;
    public string Table => table.TableCollectionName;

    public LocalizeStringKey title;
    public LocalizeStringKey detail;
    public LocalizeStringKey speech;

    public Sprite face;
    public Sprite icon;
}

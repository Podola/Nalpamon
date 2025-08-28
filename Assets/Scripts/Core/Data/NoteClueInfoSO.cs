using UnityEditor.Localization;
using UnityEngine;
using Yang.Localize;

[CreateAssetMenu(fileName = "NewClueInfoSO", menuName = "SO/Note/ClueInfo")]
public class NoteClueInfoSO : ScriptableObject, IHasID, ILocalizeFormatter
{
    [SerializeField] private string id;
    public string ID => id;

    [LocalizeTable, SerializeField] private StringTableCollection table;
    public string Table => table.TableCollectionName;

    public LocalizeStringKey title;

    [SerializeField] private SerializeDict<string[], LocalizeStringKey> detail;
    public SerializeDict<string[], LocalizeStringKey> KeyDict => detail;

    public LocalizeStringKey unowned;

    [LocalizeTable, SerializeField] private AssetTableCollection assetTable;
    public string AssetTable => assetTable.TableCollectionName;

    public ValueTrigger<LocalizeAssetKey> icon;

    public LocalizeAssetKey[] popupImage;
}
using UnityEditor.Localization;
using UnityEngine;
using Yang.Localize;

[CreateAssetMenu(fileName = "NewEnvisionInfoSO", menuName = "SO/Interro/EnvisionInfo")]
public class EnvisionInfoSO : ScriptableObject, IHasID
{
    [SerializeField] private string id;
    public string ID => id;

    public EnvisionInfo[] infos;
}

[System.Serializable]
public class EnvisionInfo
{
    [LocalizeTable, SerializeField] private StringTableCollection table;
    public string Table => table.TableCollectionName;

    [LocalizeTable, SerializeField] private AssetTableCollection assetTable;
    public string AssetTable => assetTable.TableCollectionName;

    public LocalizeAssetKey icon;

    public LocalizeStringKey title;
    public LocalizeStringKey detail;

    public bool right;
}

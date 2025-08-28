using UnityEditor.Localization;
using UnityEngine;
using Yang.Localize;

[CreateAssetMenu(fileName = "NewPhaseInfoSO", menuName = "SO/Interro/PhaseInfo")]
public class InterroPhaseInfoSO : ScriptableObject, IHasID
{
    [SerializeField] private string id;
    public string ID => id;

    [LocalizeTable, SerializeField] private StringTableCollection table;
    public string Table => table.TableCollectionName;

    public LocalizeStringKey title;
    public LocalizeStringKey main;
    public LocalizeStringKey sub;
}

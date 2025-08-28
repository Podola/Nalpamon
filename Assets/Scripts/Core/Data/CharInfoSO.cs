using UnityEngine;

[CreateAssetMenu(fileName = "NewCharInfoSO", menuName = "SO/CharInfo")]
public class CharInfoSO : ScriptableObject, IHasID
{
    [SerializeField] private string id;
    public string ID => id;

    [SerializeField] private NoteCharInfoSO noteInfo;
    public NoteCharInfoSO NoteInfo => noteInfo;

    [SerializeField] private CharStandingInfoSO standInfo;
    public CharStandingInfoSO StandInfo => standInfo;
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 각 챕터의 데이터를 관리합니다.
/// GameManager에 의해 특정 챕터의 데이터(ChapterDataSO)가 주입되면 초기화됩니다.
/// </summary>
public class ChapterDataManager : Singleton<ChapterDataManager>
{
    private ChapterDataSO _currentChapterData;
    private bool _isInitialized = false;

    // --- Public Data Properties ---
    // 이제 데이터는 GameManager가 InitializeForChapter를 호출한 후에만 접근 가능합니다.

    public Dictionary<string, InterroPhaseInfoSO> InterroPhaseDict { get; private set; }
    public Dictionary<string, EnvisionInfoSO> EnvisionDict { get; private set; }
    public Dictionary<string, NoteCharInfoSO> NoteCharDict { get; private set; }
    public Dictionary<string, NoteClueInfoSO> NoteClueDict { get; private set; }
    public Dictionary<string, Dictionary<string, StandingSpriteInfo>> StandingDict { get; private set; }

    // Note 데이터는 현재 로드된 ChapterDataSO에서 직접 가져옵니다.
    public NoteChapterInfoSO[] NoteChapter => _isInitialized ? _currentChapterData.noteChapterInfos : new NoteChapterInfoSO[0];
    public NoteClueInfoSO[] NoteClue => _isInitialized ? _currentChapterData.noteClueInfos : new NoteClueInfoSO[0];

    /// <summary>
    /// GameManager가 호출하는 초기화 메서드입니다.
    /// 제공된 ChapterDataSO를 기반으로 모든 데이터를 로드하고 딕셔너리를 설정합니다.
    /// </summary>
    public void InitializeForChapter(ChapterDataSO dataSO)
    {
        if (dataSO == null)
        {
            Log.E("초기화에 필요한 챕터 데이터(ChapterDataSO)가 없습니다.");
            _isInitialized = false;
            return;
        }

        _currentChapterData = dataSO;

        // 딕셔너리 생성
        InterroPhaseDict = new Dictionary<string, InterroPhaseInfoSO>();
        EnvisionDict = new Dictionary<string, EnvisionInfoSO>();
        NoteCharDict = new Dictionary<string, NoteCharInfoSO>();
        NoteClueDict = new Dictionary<string, NoteClueInfoSO>();
        StandingDict = new Dictionary<string, Dictionary<string, StandingSpriteInfo>>();

        // 딕셔너리 채우기
        SetDict(dataSO.interroPhaseInfos, InterroPhaseDict);
        SetDict(dataSO.envisionInfos, EnvisionDict);
        SetDict(dataSO.noteClueInfos, NoteClueDict);
        SetCharDicts(dataSO.charInfos);

        _isInitialized = true;
        Log.I($"'{dataSO.name}' 데이터로 초기화 완료.");
    }

    private void SetDict<T>(T[] infos, Dictionary<string, T> dict) where T : IHasID
    {
        if (infos == null) return;
        dict.Clear();
        foreach (T info in infos)
        {
            if (info != null && !string.IsNullOrEmpty(info.ID) && !dict.ContainsKey(info.ID))
            {
                dict.Add(info.ID, info);
            }
        }
    }

    private void SetCharDicts(CharInfoSO[] infos)
    {
        if (infos == null) return;
        NoteCharDict.Clear();
        StandingDict.Clear();
        foreach (CharInfoSO info in infos)
        {
            if (info == null || string.IsNullOrEmpty(info.ID)) continue;

            if (info.NoteInfo != null && !NoteCharDict.ContainsKey(info.ID))
                NoteCharDict.Add(info.ID, info.NoteInfo);

            if (info.StandInfo != null && !StandingDict.ContainsKey(info.ID))
                StandingDict.Add(info.ID, info.StandInfo.Convert());
        }
    }
}
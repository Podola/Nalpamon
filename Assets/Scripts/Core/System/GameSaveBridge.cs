using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임의 전체 상태를 저장하고 로드하는 과정을 총괄하는 관리자 클래스입니다.
/// ISaveable 인터페이스를 구현한 모든 객체의 상태를 수집하고 분배합니다.
/// </summary>
public sealed class GameSaveBridge : Singleton<GameSaveBridge>
{
    [Header("파일명 패턴")]
    [SerializeField] private string autoFileName = "autosave.json";
    [SerializeField] private string slotFilePattern = "slot_{0:D2}.json";

    [Header("카테고리 키")]
    [SerializeField] private string category = "game";

    [Header("로드 연출")]
    [SerializeField] private bool fadeOnLoad = true;
    [SerializeField] private float fadeSeconds = 0.25f;

    private const string SAVE_METADATA_KEY = "SaveMetadata";

    [Serializable]
    private class SaveMetadata
    {
        public string sceneToLoad;
        public string saveTime;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        if (SaveLaunchIntent.HasPending)
        {
            StartCoroutine(LoadRoutineWhenManagersReady(SaveLaunchIntent.PendingFile));
        }
    }

    /// <summary>
    /// 자동 저장을 수행합니다. StepManager의 상태만 저장합니다.
    /// </summary>
    public void SaveAutoStep()
    {
        if (StepManager.Instance == null) return;

        var stateDict = new Dictionary<string, string>();
        var saveable = StepManager.Instance as ISaveable;

        var metadata = new SaveMetadata
        {
            sceneToLoad = SceneManager.GetActiveScene().name,
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        stateDict[SAVE_METADATA_KEY] = JsonUtility.ToJson(metadata);
        stateDict[saveable.UniqueId] = JsonUtility.ToJson(saveable.CaptureState());

        // Dictionary를 직렬화 가능한 SaveContainer로 변환
        var stateList = stateDict.Select(kvp => new SaveItem { key = kvp.Key, data = kvp.Value }).ToList();
        var container = new SaveContainer { items = stateList };

        SaveLoadSystem.Add(category, "GameState", container);
        SaveLoadSystem.Save(category, autoFileName);
    }

    /// <summary>
    /// 지정된 슬롯에 수동으로 게임 전체 상태를 저장합니다.
    /// </summary>
    public void SaveManual(int slotIndex)
    {
        var file = string.Format(slotFilePattern, slotIndex);
        SaveToFile(file);
        Log.I($"수동 저장 완료: {file}");
    }

    private void SaveToFile(string fileName)
    {
        var stateDict = CaptureFullState();
        var stateList = stateDict.Select(kvp => new SaveItem { key = kvp.Key, data = kvp.Value }).ToList();
        var container = new SaveContainer { items = stateList };

        SaveLoadSystem.Add(category, "GameState", container);
        SaveLoadSystem.Save(category, fileName);
    }

    /// <summary>
    /// 파일에서 데이터를 로드하는 과정을 시작합니다.
    /// </summary>
    public void LoadFromFile(string fileName)
    {
        SaveLoadSystem.Load(category, fileName);
        var gameStateContainer = SaveLoadSystem.Get<SaveContainer>(category, "GameState");

        if (gameStateContainer != null && gameStateContainer.items != null)
        {
            var metadataItem = gameStateContainer.items.FirstOrDefault(item => item.key == SAVE_METADATA_KEY);
            if (metadataItem != null)
            {
                var metadata = JsonUtility.FromJson<SaveMetadata>(metadataItem.data);
                SaveLaunchIntent.SetPendingFile(fileName);
                SceneLoader.LoadScene(metadata.sceneToLoad);
                return;
            }
        }

        Log.E($"저장 파일({fileName})이 유효하지 않거나 메타데이터가 없습니다.");
    }

    private IEnumerator LoadRoutineWhenManagersReady(string file)
    {
        yield return new WaitUntil(() => FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<ISaveable>().Any());
        yield return null;

        SaveLoadSystem.Load(category, file);
        var gameStateContainer = SaveLoadSystem.Get<SaveContainer>(category, "GameState");
        if (gameStateContainer != null)
        {
            var stateDict = gameStateContainer.items.ToDictionary(item => item.key, item => item.data);
            RestoreFullState(stateDict);
        }

        SaveLaunchIntent.Clear();
        StepManager.Instance?.ForceRefreshAll();
        if (fadeOnLoad) yield return ScreenFader.FadeInRoutine(fadeSeconds);
        Log.I($"'{file}' 파일 로드 완료.");
    }

    /// <summary>
    /// 씬에 있는 모든 ISaveable 객체의 상태를 수집하여 딕셔너리로 반환합니다.
    /// </summary>
    private Dictionary<string, string> CaptureFullState()
    {
        var state = new Dictionary<string, string>();

        var metadata = new SaveMetadata
        {
            sceneToLoad = SceneManager.GetActiveScene().name,
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        state[SAVE_METADATA_KEY] = JsonUtility.ToJson(metadata);

        var saveables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<ISaveable>();
        foreach (var saveable in saveables)
        {
            if (!string.IsNullOrEmpty(saveable.UniqueId))
            {
                state[saveable.UniqueId] = JsonUtility.ToJson(saveable.CaptureState());
            }
        }
        return state;
    }

    /// <summary>
    /// 주어진 상태 딕셔너리를 사용하여 모든 ISaveable 객체의 상태를 복원합니다.
    /// </summary>
    private void RestoreFullState(Dictionary<string, string> state)
    {
        var saveables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None).OfType<ISaveable>();
        foreach (var saveable in saveables)
        {
            if (!string.IsNullOrEmpty(saveable.UniqueId) && state.TryGetValue(saveable.UniqueId, out string jsonState))
            {
                var stateObj = saveable.CaptureState();
                JsonUtility.FromJsonOverwrite(jsonState, stateObj);
                saveable.RestoreState(stateObj);
            }
        }
    }
}
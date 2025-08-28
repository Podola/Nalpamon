using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

/// <summary>
/// 컷신, 대화 등 이벤트 상황에서 사용될 씬별 특수 카메라들을 관리합니다.
/// </summary>
public class CameraManager : Singleton<CameraManager>
{
    private Dictionary<string, CinemachineVirtualCameraBase> _eventCameras = new Dictionary<string, CinemachineVirtualCameraBase>();

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

    private void Start()
    {
        var dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        if (dialogueRunner != null)
        {
            dialogueRunner.AddCommandHandler<string>("camera_on", ActivateCamera);
            dialogueRunner.AddCommandHandler<string>("camera_off", DeactivateCamera);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterEventCamerasInScene();
    }

    private void RegisterEventCamerasInScene()
    {
        _eventCameras.Clear();
        var eventCameraRoot = GameObject.Find("@EventCameras");
        if (eventCameraRoot == null)
        {
            Log.V("[CameraManager] 현재 씬에 '@EventCameras' 오브젝트가 없습니다.");
            return;
        }

        var cameras = eventCameraRoot.GetComponentsInChildren<CinemachineVirtualCameraBase>(true);
        foreach (var cam in cameras)
        {
            if (!_eventCameras.ContainsKey(cam.gameObject.name))
            {
                _eventCameras.Add(cam.gameObject.name, cam);
            }
            else
            {
                Log.W($"[CameraManager] 중복된 이름의 이벤트 카메라가 있습니다: {cam.gameObject.name}");
            }
        }

        Log.I($"[CameraManager] 현재 씬에서 {cameras.Length}개의 이벤트 카메라를 등록했습니다.");
    }

    public void ActivateCamera(string cameraName)
    {
        if (_eventCameras.TryGetValue(cameraName, out var cam))
        {
            cam.gameObject.SetActive(true);
            Log.V($"[CameraManager] 이벤트 카메라 활성화: {cameraName}");
        }
        else
        {
            Log.W($"[CameraManager] '{cameraName}' 이라는 이름의 이벤트 카메라를 찾을 수 없습니다.");
        }
    }

    public void DeactivateCamera(string cameraName)
    {
        if (_eventCameras.TryGetValue(cameraName, out var cam))
        {
            cam.gameObject.SetActive(false);
        }
    }
}
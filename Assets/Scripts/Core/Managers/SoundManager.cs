using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 게임의 사운드를 총괄하는 싱글톤 클래스입니다.
/// BGM과 SFX 재생을 담당하고, 설정 값을 AudioMixer에 적용합니다.
/// </summary>
public class SoundManager : Singleton<SoundManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer masterMixer;

    private SettingVolumeData _volumeData;

    protected override void Awake()
    {
        base.Awake(); // 싱글톤 Awake 로직 (DontDestroyOnLoad 포함) 호출

        // AudioSource가 인스펙터에 할당되지 않았다면 동적으로 생성
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        // 게임 시작 시 저장된 볼륨 설정을 불러와 적용
        LoadAndApplyVolumeSettings();
    }

    /// <summary>
    /// 저장된 볼륨 설정을 불러와 오디오 믹서에 적용합니다.
    /// </summary>
    public void LoadAndApplyVolumeSettings()
    {
        // SimpleSaveManager를 통해 settings.json 파일에서 볼륨 데이터 로드
        _volumeData = SaveLoadSystem.Get<SettingVolumeData>("Settings");
        _volumeData ??= new SettingVolumeData(); // 로드 실패 시 기본값으로 새 인스턴스 생성

        // AudioMixer의 노출된 파라미터에 값 설정
        // 볼륨 값(0.0001~1.0)을 데시벨(-80~0)로 변환하여 적용 (0은 -80dB로 처리)
        masterMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(_volumeData.masterVolume, 0.0001f)) * 20);
        masterMixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Max(_volumeData.bgmVolume, 0.0001f)) * 20);
        masterMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(_volumeData.sfxVolume, 0.0001f)) * 20);
    }

    /// <summary>
    /// 새로운 배경음악을 재생합니다. 현재 재생 중인 곡과 같으면 무시합니다.
    /// </summary>
    /// <param name="bgmClip">재생할 오디오 클립</param>
    public void PlayBGM(AudioClip bgmClip)
    {
        if (bgmSource.clip == bgmClip && bgmSource.isPlaying) return;

        bgmSource.clip = bgmClip;
        bgmSource.Play();
    }

    /// <summary>
    /// 효과음을 한 번 재생합니다.
    /// </summary>
    /// <param name="sfxClip">재생할 오디오 클립</param>
    public void PlaySFX(AudioClip sfxClip)
    {
        sfxSource.PlayOneShot(sfxClip);
    }
}
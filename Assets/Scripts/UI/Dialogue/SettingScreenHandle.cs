using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingScreenHandle : MonoBehaviour
{
    [Header("Windowed")]
    [SerializeField] private ConditionButton windowedButton;

    [Header("Resolution")]
    [SerializeField] private TMP_Text resolutionText;
    [SerializeField] private ConditionButton prevResolutionButton;
    [SerializeField] private ConditionButton nextResolutionButton;

    [Header("Alpha")]
    [SerializeField] private Slider dialogueAlphaSlider;

    [Header("Shake")]
    [SerializeField] private ConditionButton screenShakeButton;

    [Header("Skip")]
    [SerializeField] private ConditionButton dialogueSkipButton;

    [Header("Auto")]
    [SerializeField] private ConditionButton autoCancelButton;

    private SettingScreenData data;

    private readonly List<Resolution> resolutions = new();

    public void Init()
    {
        data = SaveLoadSystem.Get<SettingScreenData>("Settings");
        data ??= new();

        SaveLoadSystem.Add("Settings", data);

        InitResolution();

        dialogueAlphaSlider.onValueChanged.RemoveAllListeners();
        dialogueAlphaSlider.onValueChanged.AddListener(SetDialogueAlpha);

        windowedButton.AddCheck(SetWindowed);
        screenShakeButton.AddCheck(SetScreenShake);
        dialogueSkipButton.AddCheck(SetDialogueSkip);
        autoCancelButton.AddCheck(SetAutoCancel);
    }

    private bool PrevResolution(bool refresh)
    {
        bool result = true;

        if (data.resolutionIndex == 0) result = false;
        else if (!refresh)
        {
            data.resolutionIndex--;

            if (data.resolutionIndex == 0) result = false;

            SetResolution();
        }

        return result;
    }

    private bool NextResolution(bool refresh)
    {
        bool result = true;

        if (data.resolutionIndex == resolutions.Count - 1) result = false;
        else if (!refresh)
        {
            data.resolutionIndex++;

            if (data.resolutionIndex == resolutions.Count - 1) result = false;

            SetResolution();
        }

        return result;
    }

    private void InitResolution()
    {
        resolutions.Clear();

        foreach (Resolution resolution in Screen.resolutions)
        {
            if (resolution.width % 16 == 0 && resolution.height % 9 == 0) resolutions.Add(resolution);
        }

        if (data.resolutionIndex == -1)
        {
            for (int i = 0; i < resolutions.Count; i++)
            {
                if (resolutions[i].width == 1920 && resolutions[i].height == 1080)
                {
                    data.resolutionIndex = i;

                    break;
                }
            }
        }

        SetResolution();

        prevResolutionButton.AddCheck(PrevResolution);
        nextResolutionButton.AddCheck(NextResolution);
    }

    private void SetResolution()
    {
        FullScreenMode mode = data.windowed ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow;
        Resolution setResolution = resolutions[data.resolutionIndex];

        Screen.SetResolution(setResolution.width, setResolution.height, mode);

        resolutionText.text = $"{setResolution.width}*{setResolution.height}";
    }

    private void SetDialogueAlpha(float value)
    {
        data.dialogueAlpha = value;
    }

    private bool SetWindowed(bool refresh)
    {
        if (!refresh) data.windowed = !windowedButton.IsOn;

        Screen.fullScreen = windowedButton.IsOn;

        return data.windowed;
    }

    private bool SetScreenShake(bool refresh)
    {
        if (!refresh) data.screenShake = !screenShakeButton.IsOn;

        return data.screenShake;
    }

    private bool SetDialogueSkip(bool refresh)
    {
        if (!refresh) data.dialogueSkip = !dialogueSkipButton.IsOn;

        return data.dialogueSkip;
    }

    private bool SetAutoCancel(bool refresh)
    {
        if (!refresh) data.autoCancel = !autoCancelButton.IsOn;

        return data.autoCancel;
    }
}

using UnityEngine;

public class SettingVolumeHandle : MonoBehaviour
{
    private SettingVolumeData data;

    public void Init()
    {
        data = SaveLoadSystem.Get<SettingVolumeData>("Settings");
        data ??= new();

        SaveLoadSystem.Add("Settings", data);
    }
}

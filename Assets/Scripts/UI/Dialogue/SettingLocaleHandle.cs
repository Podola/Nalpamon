using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class SettingLocaleHandle : MonoBehaviour
{
    [SerializeField] private TMP_Text localeText;
    [SerializeField] private ConditionButton prevLocaleButton;
    [SerializeField] private ConditionButton nextLocaleButton;

    private SettingLocaleData data;

    private List<Locale> locales;

    public void Init()
    {
        data = SaveLoadSystem.Get<SettingLocaleData>("Settings");
        data ??= new();

        SaveLoadSystem.Add("Settings", data);

        InitLocale();
    }

    private bool PrevLocale(bool refresh)
    {
        bool result = true;

        if (data.localeIndex == 0) result = false;
        else if (!refresh)
        {
            data.localeIndex--;

            if (data.localeIndex == 0) result = false;

            SetLocaleText();
        }

        return result;
    }

    private bool NextLocale(bool refresh)
    {
        bool result = true;

        if (data.localeIndex == locales.Count - 1) result = false;
        else if (!refresh)
        {
            data.localeIndex++;
            
            if (data.localeIndex == locales.Count - 1) result = false;

            SetLocaleText();
        }

        return result;
    }

    private void InitLocale()
    {
        locales = LocalizationSettings.AvailableLocales.Locales;

        if (data.localeIndex == -1)
        {
            LocaleIdentifier identifier = new(SystemLanguage.Korean);

            for (int i = 0; i < locales.Count; i++)
            {
                if (locales[i].Identifier.Code == identifier.Code)
                {
                    data.localeIndex = i;

                    break;
                }
            }
        }

        SetLocaleText();

        prevLocaleButton.AddCheck(PrevLocale);
        nextLocaleButton.AddCheck(NextLocale);
    }

    private void SetLocaleText()
    {
        LocalizationSettings.SelectedLocale = locales[data.localeIndex];

        localeText.text = locales[data.localeIndex].LocaleName;
    }
}

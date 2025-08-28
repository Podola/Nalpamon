using System.Collections;
using TMPro;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Components;

public class ActionFadeInterro : ActionFadeBase
{
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Animator animator;

    [Header("Text Effect")]
    [SerializeField] private TMP_Text mainText;
    [SerializeField] private float effectSpeed = 5;

    [SerializeField] private StringTableCollection table;
    [SerializeField] private LocalizeStringKey mainTextData;

    [Header("Texts")]
    [SerializeField] private LocalizeStringEvent title;
    [SerializeField] private LocalizeStringEvent main;

    private string phaseID;

    protected override void Start()
    {
        base.Start();

        animator.Play("Enactive");
    }

    public override void OnEvent(string eventName)
    {
        phaseID = eventName;
    }

    protected override IEnumerator FadeIn()
    {
        InterroPhaseInfoSO infoSO = ChapterDataManager.Instance.InterroPhaseDict[phaseID];

        title.SetTable(infoSO.Table, infoSO.title);

        mainText.maxVisibleCharacters = 0;

        main.SetTable(table.TableCollectionName, mainTextData);

        animator.speed = 0;
        animator.Play("Open");

        yield return new WaitForSeconds(0.5f);

        int characterCount = mainText.textInfo.characterCount;

        if (effectSpeed <= 0 || characterCount == 0)
        {
            mainText.maxVisibleCharacters = characterCount;

            yield break;
        }

        float secondsPerLetter = 1.0f / effectSpeed;
        float accumulator = Time.deltaTime;

        while (mainText.maxVisibleCharacters < characterCount)
        {
            while (accumulator >= secondsPerLetter)
            {
                mainText.maxVisibleCharacters++;

                accumulator -= secondsPerLetter;
            }

            accumulator += Time.deltaTime;

            yield return null;
        }

        animator.speed = 1;
        mainText.maxVisibleCharacters = characterCount;

        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Active"));
    }

    protected override IEnumerator FadeOut()
    {
        animator.Play("Close");

        yield break;
    }
}

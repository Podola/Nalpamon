// 파일 경로: Scripts/2_GamePlay/World/Rooms/TimeBasedActivator.cs

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ArtRoot에 부착되어, 자식 오브젝트의 이름에 포함된 시간 태그((day), (night) 등)를
/// 기반으로 활성화 여부를 자동으로 제어합니다.
/// </summary>
public class TimeBasedActivator : MonoBehaviour
{
    // 처리할 시간 태그 목록. 인스펙터에서 수정 가능합니다.
    [SerializeField]
    private List<string> _timeTags = new List<string> { "(day)", "(afternoon)", "(night)" };

    // 성능을 위해 자식 오브젝트들을 미리 캐시해 둡니다.
    private List<GameObject> _children = new List<GameObject>();

    void Awake()
    {
        // 자신의 모든 자식 오브젝트를 리스트에 담아둡니다.
        foreach (Transform child in transform)
        {
            _children.Add(child.gameObject);
        }
    }

    void OnEnable()
    {
        // 게임 이벤트 구독
        GameEvents.OnTimeOfDayChanged += UpdateVisuals;

        // 활성화되는 순간, 현재 게임의 시간대에 맞춰 한번 업데이트 해줍니다.
        if (StepManager.Instance != null)
        {
            UpdateVisuals(StepManager.Instance.CurrentTimeOfDay);
        }
    }

    void OnDisable()
    {
        // 게임 이벤트 구독 해제
        GameEvents.OnTimeOfDayChanged -= UpdateVisuals;
    }

    /// <summary>
    /// 현재 시간대에 맞춰 올바른 오브젝트를 켜고 끕니다.
    /// </summary>
    /// <param name="currentTime">"Day", "Night" 등의 현재 시간</param>
    public void UpdateVisuals(string currentTime)
    {
        if (string.IsNullOrEmpty(currentTime)) return;

        string currentTag = $"({currentTime.ToLower()})"; // 예: "(day)"

        foreach (var child in _children)
        {
            string childName = child.name.ToLower();
            bool hasAnyTimeTag = false;

            // 오브젝트 이름에 시간 태그가 있는지 확인
            foreach (var tag in _timeTags)
            {
                if (childName.Contains(tag))
                {
                    hasAnyTimeTag = true;
                    // 현재 시간과 태그가 일치할 때만 활성화
                    child.SetActive(childName.Contains(currentTag));
                    break;
                }
            }

            // 시간 태그가 전혀 없는 오브젝트는 항상 활성화
            if (!hasAnyTimeTag)
            {
                child.SetActive(true);
            }
        }
    }
}
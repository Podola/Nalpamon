using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class DebugDisplay: MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statusText;
    public CanvasGroup _canvasGroup;
    private bool _isVisible = true;
    private void Start()
    {
        UpdateVisibility();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            _isVisible = !_isVisible;
            UpdateVisibility();
        }

        if (DebugManager.Instance == null || !DebugManager.Instance.IsDevMode || !_isVisible)
        {
            return;
        }

        UpdateStatusText();
    }

    private void UpdateVisibility()
    {
        bool show = DebugManager.Instance != null && DebugManager.Instance.IsDevMode && _isVisible;
        _canvasGroup.alpha = show ? 1f : 0f;
    }

    private void UpdateStatusText()
    {
        var sm = StepManager.Instance;
        var dm = DebugManager.Instance;

        var sb = new StringBuilder();
        // 16진수 코드로 변경하여 색상 태그 오류 해결
        sb.AppendLine($"<color=#00FFFF>-- 실시간 디버그 정보 (F1: 토글) --</color>");
        sb.AppendLine($"<b>현재 스텝:</b> {sm?.CurrentStep.ToString() ?? "N/A"}");
        sb.AppendLine($"<b>현재 방:</b> {RoomManager.Instance?.CurrentRoomType.ToString() ?? "N/A"}");

        if (dm != null && dm.RecentTrackedMarks.Any())
        {
            sb.AppendLine("--- 최근 Marks ---");
            // 이제 DebugManager가 관리하는 최근 목록을 그대로 표시
            foreach (var mark in dm.RecentTrackedMarks)
            {
                sb.AppendLine($"- {mark}");
            }
        }

        statusText.text = sb.ToString();
    }
}
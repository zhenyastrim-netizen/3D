using TMPro;
using UnityEngine;

public class RunTimerUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text phaseText;

    [Header("Timer colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = new Color(1f, 0.3f, 0.2f);
    [SerializeField, Min(0f)] private float warningTime = 60f;

    private RunManager runManager;

    private void Start()
    {
        runManager = RunManager.Instance;

        if (runManager == null)
        {
            Debug.LogError("RunTimerUI: RunManager не найден в сцене.", this);
            enabled = false;
            return;
        }

        runManager.OnTimeChanged += UpdateTimer;
        runManager.OnDayStarted += UpdateDay;
        runManager.OnPhaseChanged += UpdatePhase;

        UpdateTimer(runManager.TimeRemaining);
        UpdateDay(runManager.CurrentDay);
        UpdatePhase(runManager.Phase);
    }

    private void OnDestroy()
    {
        if (runManager == null)
            return;

        runManager.OnTimeChanged -= UpdateTimer;
        runManager.OnDayStarted -= UpdateDay;
        runManager.OnPhaseChanged -= UpdatePhase;
    }

    private void UpdateTimer(float secondsRemaining)
    {
        if (timerText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(secondsRemaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
        timerText.color = secondsRemaining <= warningTime
            ? warningColor
            : normalColor;
    }

    private void UpdateDay(int day)
    {
        if (dayText != null)
            dayText.text = $"ДЕНЬ {day} / {runManager.TotalDays}";
    }

    private void UpdatePhase(RunPhase phase)
    {
        if (phaseText == null)
            return;

        phaseText.text = phase switch
        {
            RunPhase.Exploration => "ИССЛЕДОВАНИЕ",
            RunPhase.TimeExpired => "ВРЕМЯ ВЫШЛО — ИДИТЕ К БОССУ",
            RunPhase.BossFight => "БОСС",
            RunPhase.BossDefeated => "ЗАБЕРИТЕ НАГРАДУ",
            RunPhase.RunComplete => "ЗАБЕГ ЗАВЕРШЁН",
            _ => string.Empty
        };
    }
}
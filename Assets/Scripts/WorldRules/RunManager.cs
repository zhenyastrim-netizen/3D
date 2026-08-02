using System;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [Header("Run")]
    [SerializeField, Min(1)] private int totalDays = 15;
    [SerializeField, Min(1f)] private float explorationDuration = 1800f;
    [SerializeField] private bool startAutomatically = true;

    private int currentDay;
    private float timeRemaining;
    private RunPhase phase;

    public int CurrentDay => currentDay;
    public int TotalDays => totalDays;
    public float TimeRemaining => timeRemaining;
    public RunPhase Phase => phase;
    public bool CanEnterNextDay => phase == RunPhase.BossDefeated;
public bool IsWorldActivityFrozen =>
    phase == RunPhase.TimeExpired ||
    phase == RunPhase.BossFight ||
    phase == RunPhase.BossDefeated ||
    phase == RunPhase.RunComplete;    public event Action<int> OnDayStarted;
    public event Action<float> OnTimeChanged;
    public event Action<RunPhase> OnPhaseChanged;
    public event Action OnExplorationExpired;
    public event Action OnBossDefeated;
    public event Action OnRunCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (startAutomatically && currentDay <= 0)
            StartNewRun();
    }

    private void Update()
    {
        if (phase != RunPhase.Exploration)
            return;

        timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
        OnTimeChanged?.Invoke(timeRemaining);

        if (timeRemaining <= 0f)
            ExpireExploration();
    }

    public void StartNewRun()
    {
        currentDay = 1;
        StartDay();
    }

    public void StartBossFight()
    {
        if (phase != RunPhase.Exploration &&
            phase != RunPhase.TimeExpired)
        {
            return;
        }

        SetPhase(RunPhase.BossFight);
    }

    public void RegisterBossDefeated()
    {
        if (phase == RunPhase.BossDefeated ||
            phase == RunPhase.RunComplete)
        {
            return;
        }

        SetPhase(RunPhase.BossDefeated);
        OnBossDefeated?.Invoke();
    }

    public bool TryEnterNextDay()
    {
        if (!CanEnterNextDay)
            return false;

        if (currentDay >= totalDays)
        {
            SetPhase(RunPhase.RunComplete);
            OnRunCompleted?.Invoke();
            return true;
        }

        currentDay++;
        StartDay();
        return true;
    }

    public void RestoreRun(int savedDay, float savedTime, RunPhase savedPhase)
    {
        currentDay = Mathf.Clamp(savedDay, 1, totalDays);
        timeRemaining = Mathf.Clamp(savedTime, 0f, explorationDuration);
        SetPhase(savedPhase);

        OnDayStarted?.Invoke(currentDay);
        OnTimeChanged?.Invoke(timeRemaining);
    }

    private void StartDay()
    {
        timeRemaining = explorationDuration;
        SetPhase(RunPhase.Exploration);

        OnDayStarted?.Invoke(currentDay);
        OnTimeChanged?.Invoke(timeRemaining);
    }

    private void ExpireExploration()
    {
        SetPhase(RunPhase.TimeExpired);
        OnExplorationExpired?.Invoke();
    }

    private void SetPhase(RunPhase newPhase)
    {
        if (phase == newPhase)
            return;

        phase = newPhase;
        OnPhaseChanged?.Invoke(phase);
    }
}
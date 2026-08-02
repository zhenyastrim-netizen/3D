using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DayTransitionZone : MonoBehaviour
{
    [SerializeField] private GameObject visuals;

    private Collider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
        SetAvailable(false);
    }

    private void Start()
    {
        if (RunManager.Instance == null)
        {
            Debug.LogError("RunManager не найден в сцене.", this);
            return;
        }

        RunManager.Instance.OnBossDefeated += HandleBossDefeated;
        RunManager.Instance.OnDayStarted += HandleDayStarted;

        SetAvailable(RunManager.Instance.CanEnterNextDay);
    }

    private void OnDestroy()
    {
        if (RunManager.Instance == null)
            return;

        RunManager.Instance.OnBossDefeated -= HandleBossDefeated;
        RunManager.Instance.OnDayStarted -= HandleDayStarted;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || RunManager.Instance == null)
            return;

        RunManager.Instance.TryEnterNextDay();
    }

    private void HandleBossDefeated()
    {
        SetAvailable(true);
    }

    private void HandleDayStarted(int day)
    {
        SetAvailable(false);
    }

    private void SetAvailable(bool available)
    {
        zoneCollider.enabled = available;

        if (visuals != null)
            visuals.SetActive(available);
    }
}
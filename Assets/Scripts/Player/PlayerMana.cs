using System;
using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    public float CurrentMana { get; private set; }

    public float MaxMana =>
        playerStats.GetValue(StatType.MaxMana);

    public event Action<float, float> OnManaChanged;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        CurrentMana = MaxMana;
        NotifyChanged();
    }

    private void OnEnable()
    {
        if (playerStats != null)
            playerStats.OnStatChanged += HandleStatChanged;
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnStatChanged -= HandleStatChanged;
    }

    private void Update()
    {
        float regeneration =
            playerStats.GetValue(StatType.ManaRegeneration);

        RestoreMana(regeneration * Time.deltaTime);
    }

    public bool TrySpendMana(float amount)
    {
        if (amount <= 0f)
            return true;

        if (CurrentMana < amount)
            return false;

        CurrentMana -= amount;
        NotifyChanged();

        return true;
    }

    public void RestoreMana(float amount)
    {
        if (amount <= 0f || CurrentMana >= MaxMana)
            return;

        CurrentMana = Mathf.Min(
            CurrentMana + amount,
            MaxMana
        );

        NotifyChanged();
    }

    private void HandleStatChanged(
        StatType statType,
        float newValue)
    {
        if (statType != StatType.MaxMana)
            return;

        CurrentMana = Mathf.Min(CurrentMana, MaxMana);
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        OnManaChanged?.Invoke(CurrentMana, MaxMana);
    }
}
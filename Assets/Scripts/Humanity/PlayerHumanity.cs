using System;
using UnityEngine;

public class PlayerHumanity : MonoBehaviour
{
    [Header("Humanity")]
    [SerializeField, Range(-100f, 100f)]
    private float startingValue;

    [Header("Damage bonuses")]
    [SerializeField, Min(0f)]
    private float maximumHolyBonus = 1f;

    [SerializeField, Min(0f)]
    private float maximumCursedBonus = 1f;

    private float currentValue;

    public float CurrentValue => currentValue;

    public float Humanity =>
        Mathf.Max(0f, currentValue);

    public float Corruption =>
        Mathf.Max(0f, -currentValue);

    public event Action<float> OnValueChanged;

    private void Awake()
    {
        currentValue = Mathf.Clamp(
            startingValue,
            -100f,
            100f
        );
    }

    public void ChangeHumanity(float amount)
    {
        float newValue = Mathf.Clamp(
            currentValue + amount,
            -100f,
            100f
        );

        if (Mathf.Approximately(
            currentValue,
            newValue))
        {
            return;
        }

        currentValue = newValue;
        OnValueChanged?.Invoke(currentValue);
    }

    public float GetHolyDamageMultiplier()
    {
        float normalizedHumanity =
            Humanity / 100f;

        return 1f +
            normalizedHumanity *
            maximumHolyBonus;
    }

    public float GetCursedDamageMultiplier()
    {
        float normalizedCorruption =
            Corruption / 100f;

        return 1f +
            normalizedCorruption *
            maximumCursedBonus;
    }
}
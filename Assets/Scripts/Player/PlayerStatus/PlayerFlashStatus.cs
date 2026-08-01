using System;
using System.Collections;
using UnityEngine;

public class PlayerFlashStatus : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    public bool IsBlinded { get; private set; }

    public event Action<bool> OnBlindChanged;

    private StatModifier slowModifier;
    private Coroutine effectRoutine;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
    }

    public void ApplyFlash(
        float blindDuration,
        float slowDuration,
        float slowPercent)
    {
        if (effectRoutine != null)
            StopCoroutine(effectRoutine);

        ClearEffects();

        slowModifier = new StatModifier(
            StatType.MoveSpeed,
            StatModifierType.Percent,
            -Mathf.Clamp01(slowPercent),
            this
        );

        playerStats.AddModifier(slowModifier);

        IsBlinded = true;
        OnBlindChanged?.Invoke(true);

        Debug.Log("Игрок ослеплён и замедлен");

        effectRoutine = StartCoroutine(
            FlashRoutine(
                blindDuration,
                slowDuration
            )
        );
    }

    private IEnumerator FlashRoutine(
        float blindDuration,
        float slowDuration)
    {
        float elapsed = 0f;
        float totalDuration = Mathf.Max(
            blindDuration,
            slowDuration
        );

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            if (IsBlinded &&
                elapsed >= blindDuration)
            {
                IsBlinded = false;
                OnBlindChanged?.Invoke(false);
            }

            if (slowModifier != null &&
                elapsed >= slowDuration)
            {
                playerStats.RemoveModifier(
                    slowModifier
                );

                slowModifier = null;
            }

            yield return null;
        }

        ClearEffects();
        effectRoutine = null;

        Debug.Log("Эффект световой гранаты закончился");
    }

    private void ClearEffects()
    {
        if (slowModifier != null &&
            playerStats != null)
        {
            playerStats.RemoveModifier(slowModifier);
            slowModifier = null;
        }

        if (IsBlinded)
        {
            IsBlinded = false;
            OnBlindChanged?.Invoke(false);
        }
    }

    private void OnDisable()
    {
        if (effectRoutine != null)
            StopCoroutine(effectRoutine);

        ClearEffects();
        effectRoutine = null;
    }
}
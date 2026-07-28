using UnityEngine;
using UnityEngine.InputSystem;

public class StatEffectTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerStats playerStats;

    [SerializeField]
    private StatEffect testEffect;

    private StatEffectInstance activeInstance;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // Клавиша 5 — применить эффект.
        if (Keyboard.current.digit5Key.wasPressedThisFrame)
            ApplyEffect();

        // Клавиша 6 — удалить эффект.
        if (Keyboard.current.digit6Key.wasPressedThisFrame)
            RemoveEffect();

        // Клавиша 9 — показать текущие значения.
        if (Keyboard.current.digit9Key.wasPressedThisFrame)
            PrintStats();
    }

    private void ApplyEffect()
    {
        if (playerStats == null)
        {
            Debug.LogError(
                "В StatEffectTester не указан PlayerStats.",
                this
            );

            return;
        }

        if (testEffect == null)
        {
            Debug.LogError(
                "В StatEffectTester не указан Test Effect.",
                this
            );

            return;
        }

        if (activeInstance != null &&
            activeInstance.IsActive)
        {
            Debug.Log(
                "Тестовый эффект уже активен.",
                this
            );

            return;
        }

        activeInstance = testEffect.Apply(playerStats);

        if (activeInstance == null)
        {
            Debug.LogWarning(
                "Не удалось применить тестовый эффект.",
                this
            );

            return;
        }

        Debug.Log(
            $"Эффект применён: {testEffect.EffectName}",
            this
        );

        PrintStats();
    }

    private void RemoveEffect()
    {
        if (activeInstance == null ||
            !activeInstance.IsActive)
        {
            Debug.Log(
                "Активного тестового эффекта нет.",
                this
            );

            return;
        }

        activeInstance.Remove();
        activeInstance = null;

        Debug.Log(
            "Тестовый эффект удалён.",
            this
        );

        PrintStats();
    }

    private void PrintStats()
    {
        if (playerStats == null)
            return;

        float speed =
            playerStats.GetValue(StatType.MoveSpeed);

        float maxHealth =
            playerStats.GetValue(StatType.MaxHealth);

        float rangedDamage =
            playerStats.GetValue(StatType.RangedDamage);

        Debug.Log(
            $"Move Speed: {speed} | " +
            $"Max Health: {maxHealth} | " +
            $"Ranged Damage: {rangedDamage}",
            this
        );
    }
}
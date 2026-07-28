using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Stat Effect",
    menuName = "Items/Effects/Stat Effect"
)]
public class StatEffect : ScriptableObject
{
    [Header("Information")]
    [SerializeField]
    private string effectName;

    [TextArea(2, 5)]
    [SerializeField]
    private string description;

    [Header("Stat modifications")]
    [SerializeField]
    private StatEffectEntry[] modifications;

    public string EffectName => effectName;
    public string Description => description;

    public StatEffectInstance Apply(PlayerStats targetStats)
    {
        if (targetStats == null)
        {
            Debug.LogWarning(
                $"Эффект {name} не применён: PlayerStats отсутствует.",
                this
            );

            return null;
        }

        if (modifications == null || modifications.Length == 0)
        {
            Debug.LogWarning(
                $"У эффекта {name} нет модификаторов.",
                this
            );

            return null;
        }

        List<StatModifier> appliedModifiers =
            new List<StatModifier>();

        // Создаём уникальный источник конкретного применения.
        object instanceSource = new object();

        foreach (StatEffectEntry entry in modifications)
        {
            if (entry == null)
                continue;

            StatModifier modifier = new StatModifier(
                entry.statType,
                entry.modifierType,
                entry.value,
                instanceSource
            );

            bool added = targetStats.AddModifier(modifier);

            if (added)
                appliedModifiers.Add(modifier);
        }

        if (appliedModifiers.Count == 0)
            return null;

        return new StatEffectInstance(
            this,
            targetStats,
            appliedModifiers
        );
    }
}
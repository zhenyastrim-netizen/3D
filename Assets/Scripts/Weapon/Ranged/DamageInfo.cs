using UnityEngine;

public readonly struct DamageInfo
{
    public DamagePart[] Parts { get; }
    public AttackType AttackType { get; }
    public bool IsCritical { get; }
    public GameObject Source { get; }

    public float TotalDamage
    {
        get
        {
            if (Parts == null)
                return 0f;

            float total = 0f;

            foreach (DamagePart part in Parts)
                total += part.damage;

            return total;
        }
    }

    public DamageInfo(
        DamagePart[] parts,
        AttackType attackType,
        bool isCritical,
        GameObject source)
    {
        Parts = parts;
        AttackType = attackType;
        IsCritical = isCritical;
        Source = source;
    }
}
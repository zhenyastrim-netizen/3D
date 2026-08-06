using UnityEngine;

public readonly struct CombatHitInfo
{
    public GameObject Target { get; }
    public float DamageDealt { get; }
    public AttackType AttackType { get; }
    public bool IsCritical { get; }
    public bool IsSecondary { get; }
    public bool KilledTarget { get; }
    public GameObject Source { get; }
    public DamagePart[] DamageParts { get; }

    public CombatHitInfo(
        GameObject target,
        float damageDealt,
        AttackType attackType,
        bool isCritical,
        bool isSecondary,
        bool killedTarget,
        GameObject source = null,
        DamagePart[] damageParts = null)
    {
        Target = target;
        DamageDealt = damageDealt;
        AttackType = attackType;
        IsCritical = isCritical;
        IsSecondary = isSecondary;
        KilledTarget = killedTarget;
        Source = source;
        DamageParts = damageParts != null
            ? (DamagePart[])damageParts.Clone()
            : new DamagePart[0];
    }
}

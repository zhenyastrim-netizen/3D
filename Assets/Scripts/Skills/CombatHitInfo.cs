using UnityEngine;

public readonly struct CombatHitInfo
{
    public GameObject Target { get; }
    public float DamageDealt { get; }
    public AttackType AttackType { get; }
    public bool IsCritical { get; }
    public bool IsSecondary { get; }
    public bool KilledTarget { get; }

    public CombatHitInfo(
        GameObject target,
        float damageDealt,
        AttackType attackType,
        bool isCritical,
        bool isSecondary,
        bool killedTarget)
    {
        Target = target;
        DamageDealt = damageDealt;
        AttackType = attackType;
        IsCritical = isCritical;
        IsSecondary = isSecondary;
        KilledTarget = killedTarget;
    }
}
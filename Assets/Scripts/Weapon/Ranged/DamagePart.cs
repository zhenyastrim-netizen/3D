using System;
using UnityEngine;

[Serializable]
public struct DamagePart
{
    public DamageType damageType;

    [Min(0f)]
    public float damage;

    [Tooltip("Накопление огня, холода или гниения")]
    [Min(0f)]
    public float buildup;

    public DamagePart(
        DamageType damageType,
        float damage,
        float buildup = 0f)
    {
        this.damageType = damageType;
        this.damage = damage;
        this.buildup = buildup;
    }
}
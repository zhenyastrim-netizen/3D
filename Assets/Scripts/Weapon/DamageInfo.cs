using UnityEngine;

public readonly struct DamageInfo
{
    public float Amount { get; }
    public DamageType Type { get; }
    public bool IsCritical { get; }
    public GameObject Source { get; }

    public DamageInfo(
        float amount,
        DamageType type,
        bool isCritical,
        GameObject source)
    {
        Amount = amount;
        Type = type;
        IsCritical = isCritical;
        Source = source;
    }
}
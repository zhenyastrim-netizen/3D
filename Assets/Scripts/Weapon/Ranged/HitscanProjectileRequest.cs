using UnityEngine;

public readonly struct HitscanProjectileRequest
{
    public Vector3 StartPoint { get; }
    public Vector3 Direction { get; }
    public float Range { get; }
    public DamagePart[] BaseDamageParts { get; }

    public HitscanProjectileRequest(
        Vector3 startPoint,
        Vector3 direction,
        float range,
        DamagePart[] baseDamageParts)
    {
        StartPoint = startPoint;
        Direction = direction.sqrMagnitude > 0.0001f
            ? direction.normalized
            : Vector3.forward;
        Range = Mathf.Max(0.1f, range);
        BaseDamageParts = baseDamageParts != null
            ? (DamagePart[])baseDamageParts.Clone()
            : new DamagePart[0];
    }
}

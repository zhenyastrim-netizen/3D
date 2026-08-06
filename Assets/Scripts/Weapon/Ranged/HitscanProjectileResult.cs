using UnityEngine;

public readonly struct HitscanProjectileResult
{
    public Vector3 EndPoint { get; }
    public Vector3 SurfaceNormal { get; }
    public Vector3 Direction { get; }
    public Transform HitTransform { get; }
    public IDamageable Damageable { get; }
    public bool HitSomething { get; }
    public DamagePart[] BaseDamageParts { get; }

    public HitscanProjectileResult(
        Vector3 endPoint,
        Vector3 surfaceNormal,
        Vector3 direction,
        Transform hitTransform,
        IDamageable damageable,
        bool hitSomething,
        DamagePart[] baseDamageParts)
    {
        EndPoint = endPoint;
        SurfaceNormal = surfaceNormal;
        Direction = direction;
        HitTransform = hitTransform;
        Damageable = damageable;
        HitSomething = hitSomething;
        BaseDamageParts = baseDamageParts != null
            ? (DamagePart[])baseDamageParts.Clone()
            : new DamagePart[0];
    }
}

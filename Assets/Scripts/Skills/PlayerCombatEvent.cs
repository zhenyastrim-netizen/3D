using System;
using UnityEngine;

public class PlayerCombatEvents : MonoBehaviour
{
    public event Action<CombatHitInfo> OnEnemyHit;
    public event Action<CombatHitInfo> OnEnemyKilled;
    public event Action<CombatHitInfo> OnCriticalHit;
    public event Action<float> OnDamageTaken;
    public event Action OnDashStarted;

    public void ReportEnemyHit(CombatHitInfo hitInfo)
    {
        OnEnemyHit?.Invoke(hitInfo);

        if (hitInfo.IsCritical)
            OnCriticalHit?.Invoke(hitInfo);

        if (hitInfo.KilledTarget)
            OnEnemyKilled?.Invoke(hitInfo);
    }

    public void ReportDamageTaken(float damage)
    {
        if (damage > 0f)
            OnDamageTaken?.Invoke(damage);
    }

    public void ReportDashStarted()
    {
        OnDashStarted?.Invoke();
    }
}
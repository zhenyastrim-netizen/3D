using System.Collections;
using UnityEngine;

public class ProjectileAwareness : MonoBehaviour
{
    private Coroutine registrationRoutine;
    private bool registered;

    private void OnEnable()
    {
        registrationRoutine = StartCoroutine(RegisterWhenReady());
    }

    private IEnumerator RegisterWhenReady()
    {
        while (ProjectileAwarenessSystem.Instance == null)
            yield return null;

        ProjectileAwarenessSystem.Instance.Register(transform);
        registered = true;
    }

    private void OnDisable()
    {
        if (registrationRoutine != null)
            StopCoroutine(registrationRoutine);

        if (registered && ProjectileAwarenessSystem.Instance != null)
            ProjectileAwarenessSystem.Instance.Unregister(transform);

        registered = false;
    }
}
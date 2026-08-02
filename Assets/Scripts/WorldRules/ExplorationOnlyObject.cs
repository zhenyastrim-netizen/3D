using System.Collections.Generic;
using UnityEngine;

public class ExplorationOnlyObject : MonoBehaviour
{
    [Header("What to freeze")]
    [Tooltip("Автоматически найдёт и отключит скрипты, коллайдеры, аниматоры и физику на этом объекте и его детях.")]
    [SerializeField] private bool collectAutomatically = true;
    [SerializeField] private Behaviour[] behaviours;
    [SerializeField] private Collider[] colliders;
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Rigidbody[] rigidbodies;

    private readonly Dictionary<Behaviour, bool> behaviourStates = new();
    private readonly Dictionary<Collider, bool> colliderStates = new();
    private readonly Dictionary<Renderer, bool> rendererStates = new();
    private readonly Dictionary<Rigidbody, bool> rigidbodyStates = new();

    private RunManager runManager;
    private bool isFrozen;

    private void Awake()
    {
        if (collectAutomatically)
        {
            behaviours = GetComponentsInChildren<Behaviour>(true);
            colliders = GetComponentsInChildren<Collider>(true);
            renderers = GetComponentsInChildren<Renderer>(true);
            rigidbodies = GetComponentsInChildren<Rigidbody>(true);
        }

        RememberInitialStates();
    }

    private void Start()
    {
        runManager = RunManager.Instance;

        if (runManager == null)
        {
            Debug.LogError("ExplorationOnlyObject: RunManager не найден в сцене.", this);
            return;
        }

        runManager.OnPhaseChanged += HandlePhaseChanged;
        runManager.OnDayStarted += HandleDayStarted;

        SetFrozen(runManager.IsWorldActivityFrozen);
    }

    private void OnDestroy()
    {
        if (runManager == null)
            return;

        runManager.OnPhaseChanged -= HandlePhaseChanged;
        runManager.OnDayStarted -= HandleDayStarted;
    }

    private void HandlePhaseChanged(RunPhase phase)
    {
        SetFrozen(runManager.IsWorldActivityFrozen);
    }

    private void HandleDayStarted(int day)
    {
        SetFrozen(false);
    }

    private void RememberInitialStates()
    {
        foreach (Behaviour target in behaviours)
        {
            if (target != null && target != this)
                behaviourStates[target] = target.enabled;
        }

        foreach (Collider target in colliders)
        {
            if (target != null)
                colliderStates[target] = target.enabled;
        }

        foreach (Renderer target in renderers)
        {
            if (target != null)
                rendererStates[target] = target.enabled;
        }

        foreach (Rigidbody target in rigidbodies)
        {
            if (target != null)
                rigidbodyStates[target] = target.isKinematic;
        }
    }

    private void SetFrozen(bool frozen)
    {
        if (isFrozen == frozen)
            return;

        isFrozen = frozen;

        foreach (KeyValuePair<Behaviour, bool> entry in behaviourStates)
        {
            if (entry.Key != null)
                entry.Key.enabled = frozen ? false : entry.Value;
        }

        foreach (KeyValuePair<Collider, bool> entry in colliderStates)
        {
            if (entry.Key != null)
                entry.Key.enabled = frozen ? false : entry.Value;
        }

        foreach (KeyValuePair<Renderer, bool> entry in rendererStates)
        {
            if (entry.Key != null)
                entry.Key.enabled = frozen ? false : entry.Value;
        }

        foreach (KeyValuePair<Rigidbody, bool> entry in rigidbodyStates)
        {
            if (entry.Key == null)
                continue;

            if (frozen)
            {
                entry.Key.linearVelocity = Vector3.zero;
                entry.Key.angularVelocity = Vector3.zero;
                entry.Key.isKinematic = true;
            }
            else
            {
                entry.Key.isKinematic = entry.Value;
            }
        }
    }
}
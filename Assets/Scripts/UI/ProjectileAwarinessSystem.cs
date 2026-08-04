using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProjectileAwarenessSystem : MonoBehaviour
{
    public static ProjectileAwarenessSystem Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private RectTransform indicatorContainer;
    [SerializeField] private Image indicatorPrefab;

    [Header("Awareness")]
    [SerializeField, Min(1f)] private float awarenessDistance = 12f;
    [SerializeField, Min(0f)] private float edgePadding = 65f;
    [SerializeField] private Color farColor = new Color(1f, 0.15f, 0.05f, 0.25f);
    [SerializeField] private Color nearColor = new Color(1f, 0.05f, 0.02f, 0.9f);
    [SerializeField] private Vector2 scaleRange = new Vector2(0.7f, 1.5f);

    private readonly Dictionary<Transform, Image> indicators = new();

    private void Awake()
    {
        Instance = this;

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (indicatorContainer == null)
            indicatorContainer = transform as RectTransform;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Register(Transform projectile)
    {
        if (projectile == null || indicatorPrefab == null || indicators.ContainsKey(projectile))
            return;

        Image indicator = Instantiate(indicatorPrefab, indicatorContainer);
        indicator.gameObject.SetActive(false);
        indicators.Add(projectile, indicator);
    }

    public void Unregister(Transform projectile)
    {
        if (projectile == null || !indicators.TryGetValue(projectile, out Image indicator))
            return;

        if (indicator != null)
            Destroy(indicator.gameObject);

        indicators.Remove(projectile);
    }

    private void LateUpdate()
    {
        if (playerCamera == null || indicatorContainer == null)
            return;

        foreach (KeyValuePair<Transform, Image> pair in indicators)
            UpdateIndicator(pair.Key, pair.Value);
    }

    private void UpdateIndicator(Transform projectile, Image indicator)
    {
        if (projectile == null || indicator == null)
            return;

        float distance = Vector3.Distance(playerCamera.transform.position, projectile.position);
        Vector3 viewport = playerCamera.WorldToViewportPoint(projectile.position);

        bool onScreen = viewport.z > 0f
            && viewport.x > 0f && viewport.x < 1f
            && viewport.y > 0f && viewport.y < 1f;

        bool shouldShow = distance <= awarenessDistance && !onScreen;
        indicator.gameObject.SetActive(shouldShow);

        if (!shouldShow)
            return;

        Vector2 direction = new Vector2(viewport.x - 0.5f, viewport.y - 0.5f);

        if (viewport.z < 0f)
            direction = -direction;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.down;

        direction.Normalize();

        Rect rect = indicatorContainer.rect;
        Vector2 halfSize = rect.size * 0.5f - Vector2.one * edgePadding;
        float edgeDistance = Mathf.Min(
            halfSize.x / Mathf.Max(Mathf.Abs(direction.x), 0.001f),
            halfSize.y / Mathf.Max(Mathf.Abs(direction.y), 0.001f));

        RectTransform indicatorRect = indicator.rectTransform;
        indicatorRect.anchoredPosition = direction * edgeDistance;
        indicatorRect.localRotation = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);

        float danger = 1f - Mathf.Clamp01(distance / awarenessDistance);
        indicator.color = Color.Lerp(farColor, nearColor, danger);
        indicatorRect.localScale = Vector3.one * Mathf.Lerp(scaleRange.x, scaleRange.y, danger);
    }
}
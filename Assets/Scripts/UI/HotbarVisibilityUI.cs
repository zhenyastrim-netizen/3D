using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class HotbarVisibilityUI : MonoBehaviour
{
    [SerializeField] private HotbarController hotbarController;

    [SerializeField, Min(0f)]
    private float visibleDuration = 2.5f;

    [SerializeField, Min(0.01f)]
    private float fadeDuration = 0.35f;

    private CanvasGroup canvasGroup;
    private Coroutine visibilityRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        SetVisibility(0f);
    }

    private void Start()
    {
        if (hotbarController == null)
            hotbarController =
                FindFirstObjectByType<HotbarController>();

        if (hotbarController != null)
        {
            hotbarController.OnSelectedSlotChanged +=
                HandleSelectedSlotChanged;
        }
    }

    private void OnDestroy()
    {
        if (hotbarController != null)
        {
            hotbarController.OnSelectedSlotChanged -=
                HandleSelectedSlotChanged;
        }
    }

    private void HandleSelectedSlotChanged(int index)
    {
        if (visibilityRoutine != null)
            StopCoroutine(visibilityRoutine);

        visibilityRoutine =
            StartCoroutine(VisibilityRoutine());
    }

    private IEnumerator VisibilityRoutine()
    {
        SetVisibility(1f);

        yield return new WaitForSecondsRealtime(
            visibleDuration
        );

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float alpha =
                1f - elapsed / fadeDuration;

            SetVisibility(alpha);

            yield return null;
        }

        SetVisibility(0f);
        visibilityRoutine = null;
    }

    private void SetVisibility(float alpha)
    {
        canvasGroup.alpha = alpha;
        canvasGroup.interactable = alpha > 0.99f;
        canvasGroup.blocksRaycasts = alpha > 0.99f;
    }
}
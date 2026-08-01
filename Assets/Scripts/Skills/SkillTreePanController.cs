using UnityEngine;
using UnityEngine.InputSystem;

public class SkillTreePanController : MonoBehaviour
{
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;

    [SerializeField, Min(0.1f)]
    private float panSpeed = 1f;

    private bool isPanning;
    private Vector2 lastPointerPosition;
    private Camera uiCamera;

    private void Awake()
    {
        if (viewport == null)
            viewport = transform as RectTransform;

        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas != null &&
            canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || viewport == null || content == null)
            return;

        Vector2 screenPosition =
            mouse.position.ReadValue();

        if (mouse.middleButton.wasPressedThisFrame)
        {
            bool pointerInside =
                RectTransformUtility.RectangleContainsScreenPoint(
                    viewport,
                    screenPosition,
                    uiCamera
                );

            if (!pointerInside)
                return;

            if (TryGetLocalPosition(
                    screenPosition,
                    out Vector2 localPosition))
            {
                isPanning = true;
                lastPointerPosition = localPosition;
            }
        }

        if (isPanning && mouse.middleButton.isPressed)
        {
            if (TryGetLocalPosition(
                    screenPosition,
                    out Vector2 localPosition))
            {
                Vector2 movement =
                    localPosition - lastPointerPosition;

                content.anchoredPosition +=
                    movement * panSpeed;

                lastPointerPosition = localPosition;
            }
        }

        if (mouse.middleButton.wasReleasedThisFrame)
            isPanning = false;
    }

    private bool TryGetLocalPosition(
        Vector2 screenPosition,
        out Vector2 localPosition)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewport,
            screenPosition,
            uiCamera,
            out localPosition
        );
    }

    private void OnDisable()
    {
        isPanning = false;
    }
}
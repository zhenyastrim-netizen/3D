using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(GridLayoutGroup))]
public class InventoryScrollView : MonoBehaviour
{
    [Header("Scroll")]
    [SerializeField, Min(1f)] private float viewportHeight = 350f;
    [SerializeField, Min(1f)] private float scrollSensitivity = 35f;

    private RectTransform content;
    private RectTransform viewport;
    private GridLayoutGroup grid;
    private ScrollRect scrollRect;

    private void Awake()
    {
        content = (RectTransform)transform;
        grid = GetComponent<GridLayoutGroup>();
        CreateViewport();
    }

    private IEnumerator Start()
    {
        // InventoryUI creates its slots in Start, so calculate the final
        // content height on the following frame.
        yield return null;
        RefreshLayout();
    }

    public void RefreshLayout()
    {
        if (content == null || viewport == null || grid == null)
            return;

        int columns = GetColumnCount();
        int rows = Mathf.CeilToInt(
            content.childCount / (float)columns
        );

        float rowsHeight = rows > 0
            ? rows * grid.cellSize.y +
              (rows - 1) * grid.spacing.y
            : 0f;

        float requiredHeight =
            grid.padding.top +
            grid.padding.bottom +
            rowsHeight;

        float contentHeight = Mathf.Max(
            viewport.rect.height,
            requiredHeight
        );

        content.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            contentHeight
        );

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private void CreateViewport()
    {
        Transform originalParent = content.parent;

        if (originalParent == null)
        {
            Debug.LogError(
                "InventoryScrollView requires a UI parent.",
                this
            );

            return;
        }

        int siblingIndex = content.GetSiblingIndex();

        GameObject viewportObject = new GameObject(
            "PassiveInventoryViewport",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(RectMask2D),
            typeof(ScrollRect)
        );

        viewportObject.layer = gameObject.layer;

        viewport = viewportObject.GetComponent<RectTransform>();
        viewport.SetParent(originalParent, false);
        viewport.SetSiblingIndex(siblingIndex);

        viewport.anchorMin = content.anchorMin;
        viewport.anchorMax = content.anchorMax;
        viewport.pivot = content.pivot;
        viewport.anchoredPosition = content.anchoredPosition;
        viewport.sizeDelta = new Vector2(
            content.sizeDelta.x,
            viewportHeight
        );

        Image viewportImage = viewportObject.GetComponent<Image>();
        viewportImage.color = Color.clear;
        viewportImage.raycastTarget = true;

        content.SetParent(viewport, false);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, viewportHeight);

        scrollRect = viewportObject.GetComponent<ScrollRect>();
        scrollRect.content = content;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.135f;
        scrollRect.scrollSensitivity = scrollSensitivity;
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private int GetColumnCount()
    {
        if (grid.constraint ==
            GridLayoutGroup.Constraint.FixedColumnCount)
        {
            return Mathf.Max(1, grid.constraintCount);
        }

        float availableWidth =
            viewport.rect.width -
            grid.padding.left -
            grid.padding.right;

        float cellWidth = grid.cellSize.x + grid.spacing.x;

        return Mathf.Max(
            1,
            Mathf.FloorToInt(
                (availableWidth + grid.spacing.x) / cellWidth
            )
        );
    }
}

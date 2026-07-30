
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDragManager : MonoBehaviour
{
    public static InventoryDragManager Instance { get; private set; }

    [SerializeField] private RectTransform dragRoot;
    [SerializeField] private Image dragIcon;

    public InventorySlotUI DraggedSlot { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dragRoot != null)
            dragRoot.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (dragRoot == null || !dragRoot.gameObject.activeSelf)
            return;

        dragRoot.position = Mouse.current.position.ReadValue();
    }

    public void BeginDrag(InventorySlotUI slot)
    {
        if (slot == null || slot.Icon == null)
            return;

        DraggedSlot = slot;

        dragIcon.sprite = slot.Icon.sprite;
        dragIcon.enabled = true;

        dragRoot.gameObject.SetActive(true);
        dragRoot.SetAsLastSibling();
        dragRoot.position = Mouse.current.position.ReadValue();
    }

    public void EndDrag()
    {
        DraggedSlot = null;

        if (dragRoot != null)
            dragRoot.gameObject.SetActive(false);
    }
}
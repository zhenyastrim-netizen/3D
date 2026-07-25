using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarController : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField, Min(1)] private int hotbarSize = 5;

    public int SelectedIndex { get; private set; }

    public event Action<int> OnSelectedSlotChanged;

    public InventorySlot SelectedSlot
    {
        get
        {
            if (inventory == null || inventory.Slots == null)
                return null;

            if (SelectedIndex < 0 ||
                SelectedIndex >= inventory.Slots.Length)
                return null;

            return inventory.Slots[SelectedIndex];
        }
    }

    private void Start()
    {
        SelectSlot(0);
    }

    private void Update()
    {
        ReadNumberKeys();
        ReadMouseWheel();
    }

    private void ReadNumberKeys()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SelectSlot(0);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SelectSlot(1);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            SelectSlot(2);

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            SelectSlot(3);

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
            SelectSlot(4);
    }

    private void ReadMouseWheel()
    {
        if (Mouse.current == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll > 0)
            SelectPreviousSlot();

        if (scroll < 0)
            SelectNextSlot();
    }

    public void SelectSlot(int index)
    {
        int availableSize = Mathf.Min(
            hotbarSize,
            inventory.Slots.Length
        );

        if (availableSize <= 0)
            return;

        index = Mathf.Clamp(index, 0, availableSize - 1);

        if (SelectedIndex == index)
        {
            OnSelectedSlotChanged?.Invoke(SelectedIndex);
            return;
        }

        SelectedIndex = index;
        OnSelectedSlotChanged?.Invoke(SelectedIndex);
    }

    private void SelectNextSlot()
    {
        int nextIndex = SelectedIndex + 1;

        if (nextIndex >= hotbarSize)
            nextIndex = 0;

        SelectSlot(nextIndex);
    }

    private void SelectPreviousSlot()
    {
        int previousIndex = SelectedIndex - 1;

        if (previousIndex < 0)
            previousIndex = hotbarSize - 1;

        SelectSlot(previousIndex);
    }
}
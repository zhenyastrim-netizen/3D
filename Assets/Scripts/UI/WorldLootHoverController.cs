using UnityEngine;

public class WorldLootHoverController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private WorldLootCardUI cardUI;
    [SerializeField] private float viewDistance = 5f;
    [SerializeField] private LayerMask lootMask = ~0;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                viewDistance,
                lootMask,
                QueryTriggerInteraction.Collide))
        {
            cardUI.Hide();
            return;
        }

        WorldWeaponDrop weaponDrop =
            hit.collider.GetComponentInParent<WorldWeaponDrop>();

        if (weaponDrop != null)
        {
            cardUI.ShowWeapon(
    weaponDrop.WeaponInstance
);

cardUI.SetTarget(
    weaponDrop.transform
);

return;
        }

        WorldItemDrop itemDrop =
            hit.collider.GetComponentInParent<WorldItemDrop>();

        if (itemDrop != null)
        {
            cardUI.ShowItem(
    itemDrop.Item,
    itemDrop.Amount
);

cardUI.SetTarget(
    itemDrop.transform
);

return;
        }

        cardUI.Hide();
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class HitscanWeapon : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction fireAction;

    [Header("References")]
    [SerializeField] private Camera playerCamera;

    [Header("Weapon Settings")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float fireRate = 8f;
    [SerializeField] private float range = 100f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool drawShotRay = true;

    private float nextFireTime;
    [SerializeField]private CameraRecoil recoil;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void OnEnable()
    {
        fireAction.Enable();
    }

    private void OnDisable()
    {
        fireAction.Disable();
    }

    private void Update()
    {
        if (fireAction.IsPressed() && Time.time >= nextFireTime)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        nextFireTime = Time.time + 1f / fireRate;

        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;
        recoil.Recoil(2f, 1f);

        if (Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                range,
                hitMask,
                QueryTriggerInteraction.Ignore))
        {
            IDamageable damageable =
                hit.collider.GetComponentInParent<IDamageable>();

            damageable?.TakeDamage(damage);
            

            Debug.Log($"Попадание: {hit.collider.name}");

            if (drawShotRay)
            {
                Debug.DrawLine(
                    origin,
                    hit.point,
                    Color.red,
                    1f
                );
            }
        }
        else if (drawShotRay)
        {
            Debug.DrawRay(
                origin,
                direction * range,
                Color.green,
                1f
            );
        }
    }
}
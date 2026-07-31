using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [SerializeField] private bool lockVerticalAxis = false;

    private Camera targetCamera;

    private void Awake()
    {
        targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            return;
        }

        if (lockVerticalAxis)
        {
            Vector3 direction =
                transform.position -
                targetCamera.transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
                transform.rotation =
                    Quaternion.LookRotation(direction);
        }
        else
        {
            transform.rotation =
                Quaternion.LookRotation(
                    transform.position -
                    targetCamera.transform.position,
                    targetCamera.transform.up
                );
        }
    }
}
using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    [Header("Recoil")]
    [SerializeField] private float returnSpeed = 14f;
    [SerializeField] private float recoilSpeed = 25f;

    [Header("Limits")]
    [SerializeField] private float maxVerticalRecoil = 12f;
    [SerializeField] private float maxHorizontalRecoil = 5f;

    private Vector3 currentRecoil;
    private Vector3 targetRecoil;

    private void LateUpdate()
    {
        targetRecoil = Vector3.Lerp(
            targetRecoil,
            Vector3.zero,
            returnSpeed * Time.deltaTime
        );

        currentRecoil = Vector3.Lerp(
            currentRecoil,
            targetRecoil,
            recoilSpeed * Time.deltaTime
        );

        transform.localRotation = 
    Quaternion.Euler(currentRecoil);
    }

    public void AddRecoil(float vertical, float horizontal)
    {
        float randomHorizontal = Random.Range(-horizontal, horizontal);

        targetRecoil += new Vector3(
            -vertical,
            randomHorizontal,
            0f
        );

        targetRecoil.x = Mathf.Clamp(
            targetRecoil.x,
            -maxVerticalRecoil,
            0f
        );

        targetRecoil.y = Mathf.Clamp(
            targetRecoil.y,
            -maxHorizontalRecoil,
            maxHorizontalRecoil
        );
    }
}
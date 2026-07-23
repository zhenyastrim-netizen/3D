using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float returnSpeed = 12f;
    [SerializeField] private float snappiness = 20f;

    private Vector3 currentRotation;
    private Vector3 targetRotation;

    void Update()
    {
        targetRotation = Vector3.Lerp(
            targetRotation,
            Vector3.zero,
            returnSpeed * Time.deltaTime);

        currentRotation = Vector3.Slerp(
            currentRotation,
            targetRotation,
            snappiness * Time.deltaTime);

        transform.localRotation =
            Quaternion.Euler(currentRotation);
    }

    public void Recoil(float x, float y)
    {
        targetRotation += new Vector3(
            -x,
            Random.Range(-y, y),
            0);
    }
}
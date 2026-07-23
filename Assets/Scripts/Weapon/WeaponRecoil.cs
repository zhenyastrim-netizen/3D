using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [Header("Recoil")]
    [SerializeField] private float kickBack = 0.15f;
    [SerializeField] private float kickRotation = 10f;

    [Header("Recovery")]
    [SerializeField] private float returnSpeed = 15f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private Vector3 targetPosition;
    private Quaternion targetRotation;


    private void Awake()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;

        targetPosition = startPosition;
        targetRotation = startRotation;
    }


    private void Update()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            returnSpeed * Time.deltaTime
        );

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            returnSpeed * Time.deltaTime
        );


        targetPosition = Vector3.Lerp(
            targetPosition,
            startPosition,
            returnSpeed * Time.deltaTime
        );

        targetRotation = Quaternion.Lerp(
            targetRotation,
            startRotation,
            returnSpeed * Time.deltaTime
        );
    }


    public void AddRecoil()
    {
        targetPosition += new Vector3(
            0,
            0,
            -kickBack
        );


        targetRotation *= Quaternion.Euler(
            -kickRotation,
            Random.Range(-2f,2f),
            0
        );
    }
}
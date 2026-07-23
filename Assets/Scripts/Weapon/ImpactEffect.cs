using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
    [SerializeField] private float lifeTime = 1f;

    public void Play(Vector3 position, Vector3 normal)
    {
        transform.position = position;

        transform.rotation =
            Quaternion.LookRotation(normal);

        Destroy(gameObject, lifeTime);
    }
}
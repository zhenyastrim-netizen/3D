using UnityEngine;

public class Tracer : MonoBehaviour
{
    private LineRenderer line;

    [SerializeField] private float lifeTime = 0.05f;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void Setup(Vector3 start, Vector3 end)
    {
        line.positionCount = 2;

        line.SetPosition(0, start);
        line.SetPosition(1, end);

        Destroy(gameObject, lifeTime);
    }
}
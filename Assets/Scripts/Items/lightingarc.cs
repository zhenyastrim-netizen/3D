using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightningArc : MonoBehaviour
{
    [SerializeField, Min(2)]
    private int segmentCount = 8;

    [SerializeField, Min(0f)]
    private float randomness = 0.25f;

    [SerializeField, Min(0.01f)]
    private float duration = 0.15f;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void Show(Vector3 start, Vector3 end)
    {
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            float t =
                i / (float)(segmentCount - 1);

            Vector3 point =
                Vector3.Lerp(start, end, t);

            if (i > 0 && i < segmentCount - 1)
            {
                point +=
                    Random.insideUnitSphere *
                    randomness;
            }

            lineRenderer.SetPosition(i, point);
        }

        Destroy(gameObject, duration);
    }
}
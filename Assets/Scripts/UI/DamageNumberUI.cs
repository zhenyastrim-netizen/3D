using TMPro;
using UnityEngine;

public class DamageNumberUI : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float horizontalSpeed = 1.7f;
    [SerializeField] private float upwardSpeed = 2.2f;
    [SerializeField] private float gravity = 3.5f;
    [SerializeField] private float randomSpawnOffset = 0.2f;

    private Camera playerCamera;
    private float elapsed;
    private Vector3 velocity;
    private Color baseColor;

    public void Initialize(
        float damage,
        DamageType damageType,
        bool isCritical,
        bool isSecondary)
    {
        playerCamera = Camera.main;

        damageText.text =
            Mathf.CeilToInt(damage).ToString();

        baseColor = GetColor(damageType);
        damageText.color = baseColor;

        float scale = isCritical ? 1.5f : 1f;

        if (isSecondary)
            scale *= 0.8f;

        transform.localScale *= scale;

        Vector3 right = playerCamera != null
            ? playerCamera.transform.right
            : Vector3.right;

        float side = Random.Range(-horizontalSpeed, horizontalSpeed);
        velocity = right * side + Vector3.up * Random.Range(upwardSpeed * 0.8f, upwardSpeed * 1.2f);

        transform.position += right * Random.Range(-randomSpawnOffset, randomSpawnOffset)
            + Vector3.up * Random.Range(0f, randomSpawnOffset);

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        velocity += Vector3.down * gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

        if (playerCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(
                transform.position -
                playerCamera.transform.position
            );
        }

        float alpha = 1f - elapsed / lifetime;

        Color color = baseColor;
        color.a = Mathf.SmoothStep(0f, 1f, alpha);
        damageText.color = color;
    }

    private Color GetColor(DamageType type)
    {
        return type switch
        {
            DamageType.Kinetic => Color.white,
            DamageType.Spiritual => new Color(0.7f, 0.5f, 1f),
            DamageType.Fire => new Color(1f, 0.3f, 0.05f),
            DamageType.Lightning => new Color(0.2f, 0.8f, 1f),
            DamageType.Frost => new Color(0.3f, 0.6f, 1f),
            DamageType.Decay => new Color(0.3f, 0.8f, 0.2f),
            DamageType.Holy => new Color(1f, 0.85f, 0.25f),
            DamageType.Cursed => new Color(0.65f, 0.1f, 0.8f),
            _ => Color.white
        };
    }
}
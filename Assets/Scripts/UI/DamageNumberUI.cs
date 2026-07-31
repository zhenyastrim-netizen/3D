using TMPro;
using UnityEngine;

public class DamageNumberUI : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float moveSpeed = 1.5f;

    private Camera playerCamera;
    private float elapsed;

    public void Initialize(
        float damage,
        DamageType damageType,
        bool isCritical,
        bool isSecondary)
    {
        playerCamera = Camera.main;

        damageText.text =
            Mathf.CeilToInt(damage).ToString();

        damageText.color = GetColor(damageType);

        float scale = isCritical ? 1.5f : 1f;

        if (isSecondary)
            scale *= 0.8f;

        transform.localScale *= scale;

        transform.position += new Vector3(
            Random.Range(-0.25f, 0.25f),
            Random.Range(0f, 0.25f),
            Random.Range(-0.25f, 0.25f)
        );

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        transform.position +=
            Vector3.up * moveSpeed * Time.deltaTime;

        if (playerCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(
                transform.position -
                playerCamera.transform.position
            );
        }

        float alpha = 1f - elapsed / lifetime;

        Color color = damageText.color;
        color.a = alpha;
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
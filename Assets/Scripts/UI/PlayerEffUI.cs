using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamageEffectUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Images")]
    [SerializeField] private Image damageImage;
    [SerializeField] private Image healthGateImage;

    [Header("Timing")]
    [SerializeField] private float damageFadeDuration = 0.35f;
    [SerializeField] private float gateFadeDuration = 0.8f;

    private Coroutine damageRoutine;
    private Coroutine gateRoutine;

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        SetAlpha(damageImage, 0f);
        SetAlpha(healthGateImage, 0f);

        playerHealth.OnDamaged += ShowDamage;
        playerHealth.OnHealthGateTriggered += ShowHealthGate;
    }

    private void OnDestroy()
    {
        if (playerHealth == null)
            return;

        playerHealth.OnDamaged -= ShowDamage;
        playerHealth.OnHealthGateTriggered -= ShowHealthGate;
    }

    private void ShowDamage(float damage)
    {
        if (damageRoutine != null)
            StopCoroutine(damageRoutine);

        damageRoutine = StartCoroutine(
            FadeEffect(damageImage, 0.45f, damageFadeDuration)
        );
    }

    private void ShowHealthGate()
    {
        if (gateRoutine != null)
            StopCoroutine(gateRoutine);

        gateRoutine = StartCoroutine(
            FadeEffect(healthGateImage, 0.75f, gateFadeDuration)
        );
    }

    private IEnumerator FadeEffect(
        Image image,
        float startAlpha,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float alpha = Mathf.Lerp(
                startAlpha,
                0f,
                elapsed / duration
            );

            SetAlpha(image, alpha);
            yield return null;
        }

        SetAlpha(image, 0f);
    }

    private void SetAlpha(Image image, float alpha)
    {
        if (image == null)
            return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
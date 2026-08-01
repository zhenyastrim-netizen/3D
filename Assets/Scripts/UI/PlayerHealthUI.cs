using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealthUI: PlayerHealth не найден.", this);
            return;
        }

        playerHealth.OnHealthChanged += UpdateHealth;

        UpdateHealth(
            playerHealth.CurrentHealth,
            playerHealth.MaxHealth
        );
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealth;
    }

    public void SetVisible(bool visible)
    {
        if (healthSlider != null)
            healthSlider.gameObject.SetActive(visible);

        if (healthText != null)
            healthText.gameObject.SetActive(visible);
    }

    private void UpdateHealth(float current, float maximum)
    {
        healthSlider.maxValue = maximum;
        healthSlider.value = current;

        healthText.text =
            $"{Mathf.CeilToInt(current)} / " +
            $"{Mathf.CeilToInt(maximum)}";
    }
}

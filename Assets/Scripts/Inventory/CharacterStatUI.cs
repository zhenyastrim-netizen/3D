using System.Text;
using TMPro;
using UnityEngine;

public class CharacterStatsUI : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private TMP_Text statsText;

    private void Awake()
    {
        if (playerStats == null)
        {
            playerStats =
                FindFirstObjectByType<PlayerStats>();
        }
    }

    private void OnEnable()
    {
        if (playerStats == null)
            return;

        playerStats.OnAnyStatChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnAnyStatChanged -= Refresh;
    }

    private void Refresh()
    {
        StringBuilder text = new StringBuilder();

        AddValue(
            text,
            "Максимальное здоровье",
            StatType.MaxHealth
        );

        AddValue(text, "Броня", StatType.Armor);
        AddValue(text, "Скорость", StatType.MoveSpeed);

        AddPercent(
            text,
            "Мили-урон",
            StatType.MeleeDamage
        );

        AddPercent(
            text,
            "Стрелковый урон",
            StatType.RangedDamage
        );

        AddPercent(
            text,
            "Магический урон",
            StatType.MagicDamage
        );

        AddPercent(
            text,
            "Скорость атаки",
            StatType.AttackSpeed
        );

        AddPercent(
            text,
            "Скорость перезарядки",
            StatType.ReloadSpeed
        );

        AddValue(
            text,
            "Бонус магазина",
            StatType.MagazineSize
        );

        AddChance(
            text,
            "Шанс крита",
            StatType.CriticalChance
        );

        AddPercent(
            text,
            "Критический урон",
            StatType.CriticalDamage
        );

        AddPercent(
            text,
            "Сила лечения",
            StatType.HealingPower
        );

        AddValue(text, "Удача", StatType.Luck);

        statsText.text = text.ToString();
    }

    private void AddValue(
        StringBuilder text,
        string title,
        StatType stat)
    {
        float value = playerStats.GetValue(stat);

        text.AppendLine(
            $"{title}: {value:0.##}"
        );
    }

    private void AddPercent(
        StringBuilder text,
        string title,
        StatType stat)
    {
        float multiplier = playerStats.GetValue(stat);
        float percent = multiplier * 100f;

        text.AppendLine(
            $"{title}: {percent:0.#}%"
        );
    }

    private void AddChance(
        StringBuilder text,
        string title,
        StatType stat)
    {
        float chance =
            playerStats.GetValue(stat) * 100f;

        text.AppendLine(
            $"{title}: {chance:0.#}%"
        );
    }
}
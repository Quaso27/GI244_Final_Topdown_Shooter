using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI References")]
    public Slider hpSlider;           // ลาก Slider สี่เหลี่ยมคางหมูมาใส่
    public TextMeshProUGUI hpText;    // ลาก Text ที่แสดง "HP: 100 / 100" มาใส่

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            if (GameManager.instance != null) GameManager.instance.GameOver();
        }
    }

    public void IncreaseMaxHealth(float percentAmount)
    {
        float boost = maxHealth * percentAmount;
        maxHealth += boost;
        currentHealth += boost; // เพิ่มเลือดปัจจุบันให้ด้วย
        UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        // อัปเดตหลอดเลือด Slider
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }

        // อัปเดตตัวเลข HP : 100 / 100
        if (hpText != null)
        {
            hpText.text = "HP : " + Mathf.RoundToInt(currentHealth) + " / " + Mathf.RoundToInt(maxHealth);
        }
    }
}
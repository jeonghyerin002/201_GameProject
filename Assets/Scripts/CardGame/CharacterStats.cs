using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterStats : MonoBehaviour
{
    public string characterName;
    public int maxHealth = 100;
    public int currentHealth;

    //UI ¿ä¼Ò
    public Slider healthBar;
    public TextMeshProUGUI healthText;

    public int maxMana = 10;
    public int currentMana;
    public Slider manaBar;
    public TextMeshProUGUI manaText;
    void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        UpdateUI();
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
    }

    void UpdateUI()
    {
        if (healthBar != null)
            healthBar.value = (float)currentHealth / maxHealth;

        if (healthText != null)
            healthText.text = $"{currentHealth} / {maxHealth}";

        if (manaBar != null)
            manaBar.value = (float)currentMana / maxMana;

        if (manaText != null)
            manaText.text = $"{currentMana} / {maxMana}";
    }
    public void UseMana(int amount)
    {
        currentMana -= amount;
        if (currentMana < 0)
            currentMana = 0;
        UpdateUI();
    }
    public void GainMana(int amount)
    {
        currentMana += amount;
        if (currentMana > maxMana)
            currentMana = maxMana;
        UpdateUI() ;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float _baseMaxHealth = 100f; 
    [SerializeField] private float _maxHealth;
    [SerializeField] private float _currentHealth;

    [Header("UI References")]
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private TextMeshProUGUI _hpText;

    private static PlayerHealth sharedSharedInstance;
    private bool isSharedPool = false;

    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int activePlayerCount = 0;

        foreach (GameObject p in players)
        {
            if (p.activeInHierarchy) activePlayerCount++;
        }

        if (activePlayerCount > 1)
        {
            isSharedPool = true;

            if (sharedSharedInstance == null)
            {
                sharedSharedInstance = this;
                _maxHealth = _baseMaxHealth; 
                _currentHealth = _maxHealth;
                InitializeHealthUI();
            }
            else
            {
                _maxHealth = sharedSharedInstance._maxHealth;
                _currentHealth = sharedSharedInstance._currentHealth;
            }
        }
        else
        {
            isSharedPool = false;
            _maxHealth = _baseMaxHealth;
            _currentHealth = _maxHealth;
            InitializeHealthUI();
        }
    }

    private void InitializeHealthUI()
    {
        if (_hpSlider != null)
        {
            _hpSlider.maxValue = _maxHealth;
        }
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        if (isSharedPool && sharedSharedInstance != null && sharedSharedInstance != this)
        {
            sharedSharedInstance.TakeDamage(damage);
            return;
        }

        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        UpdateHealthUI();

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.GameOver();
        }
    }

    public void IncreaseMaxHealth(float percentAmount)
    {
        if (isSharedPool && sharedSharedInstance != null && sharedSharedInstance != this)
        {
            sharedSharedInstance.IncreaseMaxHealth(percentAmount);
            return;
        }

        if (percentAmount <= 0) return;

        float boost = _maxHealth * percentAmount;
        _maxHealth += boost;
        _currentHealth += boost;

        if (_hpSlider != null) _hpSlider.maxValue = _maxHealth;

        UpdateHealthUI();
    }

    public void Heal(int amount)
    {
        if (isSharedPool && sharedSharedInstance != null && sharedSharedInstance != this)
        {
            sharedSharedInstance.Heal(amount);
            return;
        }

        if (_currentHealth >= _maxHealth || _currentHealth <= 0) return;

        _currentHealth += (float)amount;
        _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
        UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        float current = isSharedPool && sharedSharedInstance != null ? sharedSharedInstance._currentHealth : _currentHealth;
        float max = isSharedPool && sharedSharedInstance != null ? sharedSharedInstance._maxHealth : _maxHealth;

        if (_hpSlider != null)
        {
            _hpSlider.value = current;
        }

        if (_hpText != null)
        {
            _hpText.text = $"HP : {Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
        }
    }
    private void OnDestroy()
    {
        sharedSharedInstance = null;
    }
}
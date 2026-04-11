using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int score = 0;
    public int health = 3;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI healthText;
    public GameObject gameOverPanel;

    void Awake() { instance = this; } 

    void Start()
    {
        UpdateUI();
        gameOverPanel.SetActive(false);
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        UpdateUI();
        if (health <= 0) GameOver();
    }

    void UpdateUI()
    {
        scoreText.text = "Score : " + score;
        healthText.text = "HP : " + health;
    }

    void GameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0; 
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
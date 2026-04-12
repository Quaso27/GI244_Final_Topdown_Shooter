using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Stats")]
    public int score = 0;
    public int health = 3;
    public int enemiesKilled = 0;
    public float timer = 0f;

    [Header("HUD UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI healthText;

    [Header("Results UI Groups")]
    public GameObject resultsPanel;      // แผงใหญ่สุด (GameOverPanel ใน Hierarchy)
    public GameObject mainGameOverGroup; // หน้าแรก: คำว่า GAME OVER + ปุ่ม Quit
    public GameObject resultsGroup;      // หน้าสอง: สถิติ Survived/Kills + ปุ่ม DONE

    [Header("Results Texts")]
    public TextMeshProUGUI resTimeText;
    public TextMeshProUGUI resKillsText;
    public TextMeshProUGUI resWaveText;

    private bool isGameOver = false;

    void Awake() { instance = this; }

    void Start()
    {
        UpdateUI();

        // ปิดหน้าจอ GameOver ทั้งหมดตอนเริ่มเกม
        if (resultsPanel != null) resultsPanel.SetActive(false);
    }

    void Update()
    {
        if (!isGameOver)
        {
            timer += Time.deltaTime;
        }
    }

    // แก้ Error CS1061: เพิ่มฟังก์ชัน AddScore เพื่อให้กระสุนเรียกใช้ได้
    public void AddScore(int amount)
    {
        score += amount;
        UpdateUI();
    }

    public void AddKills()
    {
        enemiesKilled++;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        UpdateUI();
        if (health <= 0 && !isGameOver) GameOver();
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score : " + score;
        if (healthText != null) healthText.text = "HP : " + health;
    }

    void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0; // หยุดเวลาเกม

        // 1. คำนวณและอัปเดตสถิติ
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);
        if (resTimeText != null) resTimeText.text = string.Format("Survived: {0:00}:{1:00}", minutes, seconds);
        if (resKillsText != null) resKillsText.text = "Enemies Defeated: " + enemiesKilled;

        var spawner = Object.FindFirstObjectByType<EnemySpawner>();
        if (spawner != null && resWaveText != null)
            resWaveText.text = "Level Reached: " + spawner.currentWave;

        // 2. แสดงหน้าจอแรก (Game Over + Quit) ตามโครงสร้างใหม่
        resultsPanel.SetActive(true);
        mainGameOverGroup.SetActive(true);
        resultsGroup.SetActive(false); // ซ่อนหน้าสถิติไว้ก่อน
    }

    // ฟังก์ชันสำหรับปุ่มที่จะกดเพื่อเปลี่ยนไปหน้า Results
    public void ShowResultsPage()
    {
        mainGameOverGroup.SetActive(false); // ซ่อนหน้าแรก
        resultsGroup.SetActive(true);       // โชว์หน้าสอง (ที่มีปุ่ม DONE)
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Exited");
    }

    // ฟังก์ชัน Restart (เผื่อคุณอยากใช้ในปุ่ม DONE)
    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
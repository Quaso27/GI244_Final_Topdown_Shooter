using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Session Stats")]
    public int score = 0;
    public int enemiesKilled = 0;
    public int gold = 0;
    public int sessionGold = 0;
    private float timer = 0f;

    [Header("In-Game HUD UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI goldText;

    [Header("Pause & Upgrade UI")]
    public GameObject pauseMenuPanel; 
    public GameObject upgradePanel;  

    [Header("Results UI Groups")]
    public GameObject resultsPanel;
    public GameObject mainGameOverGroup;
    public GameObject resultsGroup;

    [Header("Results Display (Stats)")]
    public TextMeshProUGUI resTimeText;
    public TextMeshProUGUI resKillsText;
    public TextMeshProUGUI resWaveText;
    public TextMeshProUGUI resGoldText;
    public TextMeshProUGUI resSessionGoldText;

    public bool isGameOver = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        LoadGold();
    }

    void Start()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        UpdateUI();

        if (resultsPanel != null) resultsPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (upgradePanel != null) upgradePanel.SetActive(false);
    }

    void LateUpdate()
    {
        // 1. เช็กสถานะหน้าจอ
        bool isUpgradeOpen = (upgradePanel != null && upgradePanel.activeInHierarchy);
        bool isOptionOpen = (pauseMenuPanel != null && pauseMenuPanel.activeInHierarchy);

        // 2. ระบบเช็กปุ่ม Esc (เช็กเป็นอันดับท้ายๆ ของเฟรม)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isUpgradeOpen && !isGameOver)
            {
                ToggleOptions();
            }
        }

        if (isGameOver) return;

        // 3. ระบบควบคุมเวลาและ Cursor
        if (isOptionOpen || isUpgradeOpen)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            // ใช้ unscaledDeltaTime เผื่อกรณีมีการหยุดเวลาจากที่อื่น
            timer += Time.unscaledDeltaTime;
        }
    }

    // ฟังก์ชันสำหรับปุ่มฟันเฟือง (Button) และ Esc
    public void ToggleOptions()
    {
        if (isGameOver) return;

        if (pauseMenuPanel != null)
        {
            bool nextState = !pauseMenuPanel.activeSelf;
            pauseMenuPanel.SetActive(nextState);

            // ล้างค่า Focus เพื่อให้ปุ่มกดซ้ำได้ปกติ
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            Debug.Log("GameManager: Toggle " + pauseMenuPanel.name + " to " + nextState);
        }
        else
        {
            Debug.LogError("GameManager: ลืมลาก OptionsPanel ใส่ใน Inspector!");
        }
    }

    public void CloseOptions()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // --- ระบบจัดการ Score และ Gold ---
    public void AddScore(int amount) { score += amount; UpdateUI(); }
    public void AddKills() { enemiesKilled++; }
    public void AddGold(int amount) { sessionGold += amount; UpdateUI(); }

    public void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score : " + score;
        if (goldText != null) goldText.text = sessionGold.ToString();
    }

    // --- ระบบ Game Over และแสดงผลลัพธ์ ---
    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        Time.timeScale = 0f;

        if (DamageFlashEffect.instance != null) DamageFlashEffect.instance.StopAndClear();

        gold += sessionGold;
        SaveGold();
        DisplayResults();

        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
            mainGameOverGroup.SetActive(true);
            resultsGroup.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void DisplayResults()
    {
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);
        if (resTimeText != null) resTimeText.text = string.Format("Survived: {0:00}:{1:00}", minutes, seconds);
        if (resKillsText != null) resKillsText.text = "Enemies Defeated: " + enemiesKilled;
        if (resGoldText != null) resGoldText.text = gold.ToString();
        if (resSessionGoldText != null) resSessionGoldText.text = "Gold Earned: " + sessionGold;

        var spawner = Object.FindFirstObjectByType<EnemySpawner>();
        if (spawner != null && resWaveText != null) resWaveText.text = "Level Reached: " + spawner.currentWave;
    }

    public void ShowResultsPage()
    {
        if (mainGameOverGroup != null) mainGameOverGroup.SetActive(false);
        if (resultsGroup != null) resultsGroup.SetActive(true);
    }

    public void SaveGold() { PlayerPrefs.SetInt("TotalGold", gold); PlayerPrefs.Save(); }
    public void LoadGold() { gold = PlayerPrefs.GetInt("TotalGold", 0); }
    public void RestartGame() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void QuitGame() { SaveGold(); Application.Quit(); }
}
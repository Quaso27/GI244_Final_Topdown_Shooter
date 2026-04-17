using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Scaling Constants")]
    public float monsterDensityFactor = 1.2f; // ยิ่งมาก มอนยิ่งเพิ่มเร็วแบบทวีคูณ
    public float timeScalingFactor = 0.8f;    // ยิ่งน้อย เวลาจะบีบคั้นขึ้นในเวฟหลังๆ

    [Header("Base Settings")]
    public GameObject enemyPrefab;
    public int currentWave = 1;
    public float baseWaveTime = 15f;
    public int initialEnemies = 5;

    private float waveTimer;
    private bool waitingForNextWave = false;

    public float minSpawnRadius = 12f; // ระยะห่างขั้นต่ำ (ให้อยู่นอกขอบจอ)
    public float maxSpawnRadius = 15f;

    void Start() => StartNewWave();

    void Update()
    {
        if (waitingForNextWave) return;

        int currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        waveTimer -= Time.deltaTime;

        if (currentEnemies <= 0)
            StartCoroutine(NextWaveRoutine(5f));
        else if (waveTimer <= 0)
            StartCoroutine(NextWaveRoutine(0f));
    }

    void StartNewWave()
    {
        // 1. คำนวณจำนวนมอนสเตอร์ (Exponential)
        // สูตร: มอนสเตอร์จะเพิ่มขึ้นแบบก้าวกระโดดในเวฟหลังๆ
        int countToSpawn = initialEnemies + Mathf.RoundToInt(Mathf.Pow(currentWave, monsterDensityFactor));

        // 2. คำนวณเวลาแบบ Scaled (ใช้ Log เพื่อไม่ให้เวลานานเกินไปจนน่าเบื่อ)
        // สูตร: เวลาพื้นฐาน + (จำนวนมอนสเตอร์ ^ 0.8)
        float dynamicTime = baseWaveTime + Mathf.Pow(countToSpawn, timeScalingFactor);
        waveTimer = dynamicTime;

        Debug.Log($"Wave {currentWave}: {countToSpawn} Enemies | Time: {dynamicTime:F1}s");

        for (int i = 0; i < countToSpawn; i++) SpawnEnemy();
    }

    // ... ส่วนของ NextWaveRoutine และ SpawnEnemy เหมือนเดิม ...
    IEnumerator NextWaveRoutine(float delay)
    {
        waitingForNextWave = true;
        yield return new WaitForSeconds(delay);
        currentWave++;
        StartNewWave();
        waitingForNextWave = false;
    }

    void SpawnEnemy()
    {
        Vector3 spawnPos = Vector3.zero;
        bool isValidPosition = false;
        int maxAttemptes = 20;
        int attempts = 0;

        while (!isValidPosition && attempts < maxAttemptes)
        {
            float randomX = Random.Range(-20f, 18f);
            float randomY = Random.Range(-7f, 5f);
            spawnPos = new Vector3(randomX, randomY, 0);

            Vector3 screenPoint = Camera.main.WorldToViewportPoint(spawnPos);
            bool isOffScreen = screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1;

            if (isOffScreen)
            { 
                isValidPosition = true;            
            }
            attempts++;
        }
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}
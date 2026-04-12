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
        // 1. หาตำแหน่งของผู้เล่น
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 playerPos = (player != null) ? player.transform.position : Vector3.zero;

        // 2. สุ่มมุม 360 องศา (เป็นเรเดียน)
        float angle = Random.Range(0f, Mathf.PI * 2);

        // 3. สุ่มระยะห่างระหว่างรัศมีขั้นต่ำและสูงสุด
        float distance = Random.Range(minSpawnRadius, maxSpawnRadius);

        // 4. คำนวณจุดเกิดจากมุมและระยะทาง (สูตรตรีโกณมิติ)
        float spawnX = Mathf.Cos(angle) * distance;
        float spawnY = Mathf.Sin(angle) * distance;

        Vector3 spawnPos = new Vector3(playerPos.x + spawnX, playerPos.y + spawnY, 0);

        // 5. เสกมอนสเตอร์
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}
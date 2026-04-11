using UnityEngine;
using System.Collections; // ต้องมีบรรทัดนี้เพื่อใช้คำสั่งรอ (Coroutine)

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnDistance = 12f;

    public int currentWave = 0;
    private int enemiesToSpawn;
    private bool isSpawning = false;

    void Update()
    {
        // เช็คว่าศัตรูในฉากตายหมดหรือยัง และไม่ได้กำลังเสกเวฟใหม่อยู่
        int currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (currentEnemies <= 0 && !isSpawning)
        {
            StartCoroutine(StartNextWave());
        }
    }

    IEnumerator StartNextWave()
    {
        isSpawning = true;
        currentWave++; // เพิ่มเลขเวฟ

        Debug.Log("เริ่ม Wave: " + currentWave);

        // สูตรคำนวณจำนวนศัตรู (เช่น เวฟที่ 1 มี 5 ตัว, เวฟ 2 มี 8 ตัว)
        enemiesToSpawn = 2 + (currentWave * 3);

        // รอสัก 2 วินาทีก่อนเริ่มเสก (ให้ผู้เล่นเตรียมตัว)
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
            // เวลาระหว่างเสกแต่ละตัวในเวฟเดียวกัน (ยิ่งเวฟสูง ยิ่งเสกไวขึ้น)
            yield return new WaitForSeconds(0.5f);
        }

        isSpawning = false;
    }

    void SpawnEnemy()
    {
        Vector2 randomPos = Random.insideUnitCircle.normalized * spawnDistance;
        Vector3 spawnPos = new Vector3(randomPos.x, randomPos.y, 0f) + transform.position;
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}
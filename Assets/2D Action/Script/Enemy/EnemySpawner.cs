using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyType
    {
        public GameObject enemyPrefab;
        public int weight;
    }

    [Header("Enemy Configuration")]
    public EnemyType[] enemies;
    public int initialEnemies = 5;
    public int currentWave = 1;
    public float baseWaveTime = 15f;

    [Header("Layer Settings")]
    public LayerMask floorLayer;
    public LayerMask wallLayer;
    public LayerMask enemyLayer;

    private float waveTimer;
    private bool waitingForNextWave = false;
    private int totalToSpawn;
    private int totalKilled;
    private Transform player;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // กันพลาด: ถ้าไม่ได้ตั้ง Layer ใน Inspector ให้ดึงจากชื่อ "Enemy"
        if (enemyLayer == 0) enemyLayer = LayerMask.GetMask("Enemy");

        StartNewWave();
    }

    void Update()
    {
        if (waitingForNextWave || player == null) return;

        waveTimer -= Time.deltaTime;
        int currentEnemiesInScene = GameObject.FindGameObjectsWithTag("Enemy").Length;

        // จบเวฟเมื่อฆ่าครบ หรือ เวลาในเวฟหมด
        if ((currentEnemiesInScene <= 0 && totalKilled >= totalToSpawn) || waveTimer <= 0)
        {
            StartCoroutine(NextWaveRoutine(waveTimer <= 0 ? 0f : 0.5f));
        }
    }

    void StartNewWave()
    {
        totalKilled = 0;
        totalToSpawn = initialEnemies + Mathf.RoundToInt(Mathf.Pow(currentWave, 1.2f));
        waveTimer = baseWaveTime + Mathf.Pow(totalToSpawn, 0.8f);

        Debug.Log($"Wave {currentWave} Started! Target: {totalToSpawn} Enemies.");
        StartCoroutine(SpawnWaveRoutine(totalToSpawn));
    }

    IEnumerator SpawnWaveRoutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(0.15f);
        }
    }

    public void RecordEnemyDeath() { totalKilled++; }

    void SpawnEnemy()
    {
        GameObject prefab = GetRandomEnemyByWeight();
        if (prefab == null) return;

        bool isValidPosition = false;
        int attempts = 0;
        Vector3 finalSpawnPos = Vector3.zero;

        // พยายามสุ่มหาที่ว่างรอบตัวผู้เล่น
        while (!isValidPosition && attempts < 50)
        {
            attempts++;

            // ใช้การสุ่มมุม 360 องศาเพื่อให้กระจายตัวทั่วแผนที่ ไม่กองที่จุดใดจุดหนึ่ง
            float randomAngle = Random.Range(0f, 360f);
            Vector3 direction = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad), 0);
            float randomDist = Random.Range(11f, 15f); // ระยะที่พ้นขอบจอพอดีแต่ไม่ไกลเกินไป

            Vector3 candidatePos = player.position + (direction * randomDist);

            // 1. เช็คว่าอยู่นอกสายตา
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(candidatePos);
            bool isOffScreen = viewportPos.x < -0.05f || viewportPos.x > 1.05f || viewportPos.y < -0.05f || viewportPos.y > 1.05f;

            // 2. เช็คว่าอยู่บนพื้น และ ไม่ชนกำแพง
            bool onFloor = Physics2D.OverlapCircle(candidatePos, 0.3f, floorLayer);
            bool noWall = !Physics2D.OverlapCircle(candidatePos, 0.3f, wallLayer);

            // 3. เช็คว่าไม่ทับกับมอนสเตอร์ตัวอื่น (Overlap Check)
            bool noEnemyOverlap = !Physics2D.OverlapCircle(candidatePos, 0.8f, enemyLayer);

            if (isOffScreen && onFloor && noWall && noEnemyOverlap)
            {
                finalSpawnPos = candidatePos;
                isValidPosition = true;
            }
        }

        if (isValidPosition)
        {
            Instantiate(prefab, finalSpawnPos, Quaternion.identity);
        }
    }

    GameObject GetRandomEnemyByWeight()
    {
        int totalWeight = 0;
        foreach (var enemy in enemies) totalWeight += enemy.weight;
        if (totalWeight == 0) return null;

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;
        foreach (var enemy in enemies)
        {
            currentWeight += enemy.weight;
            if (randomValue < currentWeight) return enemy.enemyPrefab;
        }
        return null;
    }

    IEnumerator NextWaveRoutine(float delay)
    {
        if (waitingForNextWave) yield break;
        waitingForNextWave = true;

        yield return new WaitForSeconds(delay);

        if (AugmentManager.instance != null)
        {
            AugmentManager.instance.OnWaveCleared(currentWave);
            // หยุดรอจนกว่าผู้เล่นจะเลือกการ์ดเสร็จ (เมนูปิดลง)
            while (AugmentManager.instance.isMenuOpen)
            {
                yield return null;
            }
        }

        currentWave++;
        StartNewWave();
        waitingForNextWave = false;
    }
}
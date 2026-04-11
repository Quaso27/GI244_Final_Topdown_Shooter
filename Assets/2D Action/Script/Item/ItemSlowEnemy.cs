using System.Collections;
using UnityEngine;

public class ItemSlowEnemy : MonoBehaviour
{
    public float slowAmount = 2f;    // จะเอาความเร็วศัตรูมา "ลบ" ออกเท่าไหร่
    public float duration = 5f;      // ระยะเวลาสโลว์
    public float despawnTime = 10f;  // เวลาที่ไอเทมจะหายไปเองถ้าไม่เก็บ

    private void Start()
    {
        // ให้ไอเทมหายไปเองถ้าไม่มีคนเก็บ
        Destroy(gameObject, despawnTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // ยกเลิกการลบตัวเองทิ้ง เพื่อให้ Coroutine ทำงานจนจบ
            CancelInvoke();

            StartCoroutine(SlowRoutine());

            // ซ่อนไอเทมทันที
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;
        }
    }

    IEnumerator SlowRoutine()
    {
        // 1. หาศัตรูทุกตัวในฉากที่มีสคริปต์ EnemyFollow
        EnemyFollow[] enemies = Object.FindObjectsByType<EnemyFollow>(FindObjectsSortMode.None);

        // 2. ลดความเร็วศัตรูทุกตัวที่เจอ
        foreach (EnemyFollow enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.speed -= slowAmount;
                // ป้องกันไม่ให้ความเร็วติดลบ (ถ้าติดลบศัตรูจะเดินถอยหลัง)
                if (enemy.speed < 0.5f) enemy.speed = 0.5f;
            }
        }

        Debug.Log("Enemies Slowed down!");

        // 3. รอจนครบเวลา
        yield return new WaitForSeconds(duration);

        // 4. คืนความเร็ว (ต้องหาใหม่อีกรอบ เพราะอาจมีศัตรูตัวเก่าที่ยังไม่ตายเหลืออยู่)
        EnemyFollow[] enemiesToRestore = Object.FindObjectsByType<EnemyFollow>(FindObjectsSortMode.None);
        foreach (EnemyFollow enemy in enemiesToRestore)
        {
            if (enemy != null)
            {
                enemy.speed += slowAmount;
            }
        }

        Debug.Log("Enemies back to normal speed.");
        Destroy(gameObject);
    }
}
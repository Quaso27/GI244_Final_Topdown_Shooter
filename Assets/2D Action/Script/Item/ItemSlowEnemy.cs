using System.Collections;
using System.Collections.Generic; // ต้องใช้ List
using UnityEngine;

public class ItemSlowEnemy : MonoBehaviour
{
    public float slowAmount = 2f;
    public float duration = 5f;
    public float despawnTime = 10f;

    private bool isCollected = false;

    private void Start()
    {
        // ใช้ Invoke เพื่อให้ยกเลิกได้ชัวร์กว่า Destroy ตรงๆ
        Invoke("SelfDestroy", despawnTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true;
            CancelInvoke("SelfDestroy");

            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;

            StartCoroutine(SlowRoutine());
        }
    }

    private void SelfDestroy()
    {
        if (!isCollected) Destroy(gameObject);
    }

    IEnumerator SlowRoutine()
    {
        // เก็บรายชื่อมอนสเตอร์ที่ "โดนลดความเร็วไปจริงๆ" ไว้ในลิสต์
        List<EnemySlime> slowedSlimes = new List<EnemySlime>();

        EnemyBase[] enemies = Object.FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);

        foreach (EnemyBase enemy in enemies)
        {
            if (enemy is EnemySlime slime)
            {
                slime.moveSpeed -= slowAmount;
                if (slime.moveSpeed < 0.5f) slime.moveSpeed = 0.5f;

                slowedSlimes.Add(slime); // บันทึกไว้ว่าตัวนี้โดนสโลว์นะ
            }
        }

        Debug.Log($"Slowed down {slowedSlimes.Count} enemies!");

        yield return new WaitForSeconds(duration);

        // คืนความเร็ว "เฉพาะตัวที่อยู่ในลิสต์" และ "ยังไม่ตาย" เท่านั้น
        foreach (EnemySlime slime in slowedSlimes)
        {
            if (slime != null) // เช็กว่ามอนสเตอร์ยังไม่ถูก Destroy ไปก่อน
            {
                slime.moveSpeed += slowAmount;
            }
        }

        Debug.Log("Restored speed to original enemies.");
        Destroy(gameObject);
    }
}
using System.Collections;
using UnityEngine;

public class ItemSpeedBoost : MonoBehaviour
{
    public float speedMultiplier = 5f; // เปลี่ยนชื่อเป็น boostAmount จะสื่อความหมายกว่า
    public float duration = 3f;
    public float despawnTime = 10f; // เวลาที่ไอเทมจะหายไปถ้าไม่เก็บ

    private void Start()
    {
        // เริ่มนับถอยหลังลบตัวเองตั้งแต่เกิด
        Destroy(gameObject, despawnTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                // ยกเลิกการ Destroy(gameObject, despawnTime) ที่ตั้งไว้ใน Start
                // เพื่อไม่ให้มันหายไปกลางคันขณะกำลังให้บัฟ
                CancelInvoke();

                StartCoroutine(SpeedBoostRoutine(pc));
            }

            // ซ่อนไอเทมทันที
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<Collider2D>().enabled = false;
        }
    }

    IEnumerator SpeedBoostRoutine(PlayerController pc)
    {
        pc.moveSpeed += speedMultiplier;
        Debug.Log("Speed Up! Current Speed: " + pc.moveSpeed);

        yield return new WaitForSeconds(duration);

        if (pc != null) // เช็คเผื่อ Player ตายไปก่อนบัฟหมด
        {
            pc.moveSpeed -= speedMultiplier;
            Debug.Log("Speed Normal. Current Speed: " + pc.moveSpeed);
        }

        Destroy(gameObject);
    }
}
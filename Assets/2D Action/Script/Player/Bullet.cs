using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 20f;

    [Header("Orientation Fix")]
    [Tooltip("ถ้ากระสุนออกมาผิดด้าน ให้ลองปรับมุมตรงนี้ เช่น 90 หรือ -90")]
    public float spriteRotationOffset = 0f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 1. สั่งให้กระสุนพุ่งไปทางด้านหน้าของวัตถุ (ใน 2D คือทิศทางที่หัวลูกศรสีเขียว/แดงชี้ไปตอน Spawn)
        // โดยปกติระบบยิงที่เราทำจะ Spawn กระสุนให้หันไปหาเมาส์อยู่แล้ว
        rb.linearVelocity = transform.up * speed;

        // 2. ปรับทิศทางของ Sprite ให้ตรงกับทิศทางการพุ่ง
        // หมุนตัวมันเองตามค่า Offset ที่ตั้งไว้ใน Inspector
        transform.Rotate(0, 0, spriteRotationOffset);

        // ทำลายตัวเองทิ้งเมื่อเวลาผ่านไป 2 วินาที
        Destroy(gameObject, 2f);
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // เช็คว่าชนศัตรูหรือไม่
        if (hitInfo.CompareTag("Enemy"))
        {
            EnemyBase enemy = hitInfo.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(1); // ทำดาเมจ 1 หน่วย
            }
            Destroy(gameObject); // ชนแล้วทำลายกระสุน
        }

        // เช็คว่าชนกำแพงหรือไม่
        if (hitInfo.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
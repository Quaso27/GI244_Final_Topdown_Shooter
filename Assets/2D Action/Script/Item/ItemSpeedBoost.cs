using System.Collections;
using UnityEngine;

public class ItemSpeedBoost : MonoBehaviour
{
    public float speedMultiplier = 5f;
    public float duration = 3f;
    public float despawnTime = 10f;

    private bool isCollected = false;

    private void Start()
    {
        // ใช้คำสั่งนี้แทนเพื่อเริ่มนับถอยหลัง
        Invoke("SelfDestroy", despawnTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // เพิ่ม !isCollected เพื่อป้องกันการเก็บซ้อนกัน
        if (other.CompareTag("Player") && !isCollected)
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                isCollected = true;
                // ยกเลิกการลบตัวเองที่ตั้งไว้ใน Start
                CancelInvoke("SelfDestroy");

                // ซ่อนไอเทมและปิด Collider ทันทีเพื่อให้เก็บได้แค่ครั้งเดียว
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<Collider2D>().enabled = false;

                StartCoroutine(SpeedBoostRoutine(pc));
            }
        }
    }

    private void SelfDestroy()
    {
        if (!isCollected) Destroy(gameObject);
    }

    IEnumerator SpeedBoostRoutine(PlayerController pc)
    {
        // เก็บค่าความเร็วเดิมไว้ เพื่อความชัวร์เวลาลดค่าคืน
        pc.moveSpeed += speedMultiplier;
        Debug.Log("Speed Up! Current Speed: " + pc.moveSpeed);

        // ใช้ WaitForSecondsRealtime ถ้าเกมคุณมีการ Pause (Time.timeScale = 0)
        yield return new WaitForSeconds(duration);

        if (pc != null)
        {
            pc.moveSpeed -= speedMultiplier;
            Debug.Log("Speed Normal. Current Speed: " + pc.moveSpeed);
        }

        // ลบ Object ทิ้งหลังจากคืนค่าความเร็วเสร็จแล้วเท่านั้น
        Destroy(gameObject);
    }
}
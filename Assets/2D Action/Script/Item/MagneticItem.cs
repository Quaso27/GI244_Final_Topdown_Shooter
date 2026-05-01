using UnityEngine;

public class MagneticItem : MonoBehaviour
{
    [Header("Magnet Settings")]
    public float moveSpeed = 5f;        // ความเร็วเริ่มต้นที่พุ่งหาผู้เล่น
    public float acceleration = 2f;    // ความเร็วจะเพิ่มขึ้นเรื่อยๆ ขณะพุ่งหา

    private Transform targetPlayer;
    private bool isBeingPulled = false;
    private float currentSpeed;

    void Start()
    {
        currentSpeed = moveSpeed;
    }

    void Update()
    {
        if (isBeingPulled && targetPlayer != null)
        {
            // คำนวณทิศทางหาผู้เล่น
            Vector3 direction = (targetPlayer.position - transform.position).normalized;

            // เร่งความเร็วขึ้นเรื่อยๆ เพื่อความสะใจ
            currentSpeed += acceleration * Time.deltaTime;

            // พุ่งไปหาผู้เล่น
            transform.position += direction * currentSpeed * Time.deltaTime;
        }
    }

    // ฟังก์ชันนี้จะถูกเรียกโดยวงรัศมีของผู้เล่น
    public void StartPull(Transform playerTransform)
    {
        if (!isBeingPulled)
        {
            targetPlayer = playerTransform;
            isBeingPulled = true;
        }
    }
}
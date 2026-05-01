using UnityEngine;

public class SmartEnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float detectionRadius = 2.5f; // เพิ่มระยะให้มองไกลขึ้น
    public float bodyRadius = 0.4f;      // ขนาดรัศมีตัวมอนสเตอร์ (ช่วยให้ไม่เบียดมุม)
    public LayerMask obstacleLayer;

    [Header("AI Behavior")]
    [Range(0, 1)] public float momentumWeight = 0.2f; // น้ำหนักของทิศทางเดิม (ช่วยลดการสั่น)

    private Rigidbody2D rb;
    private Transform player;
    private Vector2 lastDirection;

    // ทิศทาง 8 ทิศ
    private readonly Vector2[] directions = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right,
        new Vector2(1,1).normalized, new Vector2(1,-1).normalized,
        new Vector2(-1,1).normalized, new Vector2(-1,-1).normalized
    };

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player")?.transform;

        // ล็อคการหมุนของ Rigidbody
        if (rb != null) rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 bestDir = CalculateContextSteering();

        // เคลื่อนที่ด้วยความเร็ว
        rb.linearVelocity = bestDir * moveSpeed;

        // หันหน้าตามทิศที่เดิน (Flip Sprite)
        if (bestDir.x != 0)
        {
            transform.localScale = new Vector3(bestDir.x > 0 ? 1 : -1, 1, 1);
        }
    }

    Vector2 CalculateContextSteering()
    {
        float[] interests = new float[8];
        float[] dangers = new float[8];

        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        for (int i = 0; i < 8; i++)
        {
            // 1. คำนวณความอยากไป (Interest)
            float d = Vector2.Dot(directions[i], directionToPlayer);

            // เพิ่ม Momentum: ให้คะแนนพิเศษกับทิศที่เพิ่งเดินมา เพื่อให้เดินไถกำแพงพริ้วขึ้น
            float momentum = Vector2.Dot(directions[i], lastDirection) * momentumWeight;

            interests[i] = Mathf.Max(0, d + momentum);

            // 2. คำนวณอันตราย (Danger) ด้วย CircleCast
            // CircleCast จะเช็คเป็นวงกลมตามขนาดตัวมอนสเตอร์ ทำให้ไม่เดินเบียดขอบกำแพง
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, bodyRadius, directions[i], detectionRadius, obstacleLayer);

            if (hit.collider != null)
            {
                // ยิ่งใกล้กำแพง คะแนนอันตรายยิ่งสูง
                dangers[i] = 1.0f - (hit.distance / detectionRadius);
            }
        }

        // 3. หักลบค่าและรวมทิศทาง
        Vector2 outputDirection = Vector2.zero;
        for (int i = 0; i < 8; i++)
        {
            interests[i] = Mathf.Clamp01(interests[i] - dangers[i]);
            outputDirection += directions[i] * interests[i];
        }

        // เก็บข้อมูลทิศทางล่าสุดไว้ใช้ใน Frame ถัดไป
        if (outputDirection != Vector2.zero)
        {
            lastDirection = outputDirection.normalized;
        }

        return outputDirection.normalized;
    }

    // ช่วยวาดเส้นในหน้า Scene เพื่อดูว่า AI กำลังคิดอะไรอยู่
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, bodyRadius);
    }
}
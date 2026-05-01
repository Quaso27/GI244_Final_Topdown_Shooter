using UnityEngine;

// สืบทอดจาก EnemyBase
public class EnemySlime : EnemyBase
{
    [Header("AI Movement Settings")]
    public float moveSpeed = 2f;
    public float detectionRadius = 1.5f; // ระยะมองเห็นกำแพง (แนะนำ 1.5 - 2.0)
    public float bodyRadius = 0.3f;      // รัศมีตัวมอนสเตอร์ (ช่วยให้ไม่เบียดมุม)
    public LayerMask obstacleLayer;      // เลือก Layer "Walls"

    [Range(0, 1)] public float momentumWeight = 0.15f; // ช่วยให้เดินเลาะกำแพงพริ้วขึ้น

    private Transform player;
    private Vector2 lastDirection;
    private readonly Vector2[] directions = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right,
        new Vector2(1,1).normalized, new Vector2(1,-1).normalized,
        new Vector2(-1,1).normalized, new Vector2(-1,-1).normalized
    };

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // ล็อคการหมุนของ Rigidbody
        if (rb != null) rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (isDead || player == null || isStunned) return;

        // --- ใช้ระบบ Smart AI ในการเดินปกติ ---
        Vector2 smartDir = CalculateContextSteering();
        rb.linearVelocity = smartDir * moveSpeed;

        // กลับด้าน Sprite ตามทิศทาง x
        UpdateFacing(rb.linearVelocity.x);
    }

    // ระบบ AI คำนวณทิศทาง (Context Steering)
    Vector2 CalculateContextSteering()
    {
        float[] interests = new float[8];
        float[] dangers = new float[8];
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        for (int i = 0; i < 8; i++)
        {
            // 1. Interest: คะแนนความอยากไปหาผู้เล่น
            float d = Vector2.Dot(directions[i], directionToPlayer);

            // เพิ่ม Momentum: ช่วยให้เดินสมูทขึ้น ไม่เปลี่ยนทิศทางกะทันหัน
            float momentum = Vector2.Dot(directions[i], lastDirection) * momentumWeight;
            interests[i] = Mathf.Max(0, d + momentum);

            // 2. Danger: เช็คสิ่งกีดขวางด้วย CircleCast
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, bodyRadius, directions[i], detectionRadius, obstacleLayer);
            if (hit.collider != null)
            {
                // ยิ่งใกล้สิ่งกีดขวาง คะแนนอันตรายยิ่งสูง
                dangers[i] = 1.0f - (hit.distance / detectionRadius);
            }
        }

        // 3. รวมทิศทาง (Vector Summation)
        Vector2 outputDirection = Vector2.zero;
        for (int i = 0; i < 8; i++)
        {
            // หักลบค่าอันตรายออกจากความสนใจ
            interests[i] = Mathf.Clamp01(interests[i] - dangers[i]);
            outputDirection += directions[i] * interests[i];
        }

        // บันทึกทิศทางล่าสุด
        if (outputDirection.sqrMagnitude > 0.01f)
        {
            lastDirection = outputDirection.normalized;
        }

        // ป้องกันค่า NaN และส่งทิศทางที่รวมแล้วกลับไป
        return outputDirection.sqrMagnitude > 0.01f ? outputDirection.normalized : Vector2.zero;
    }

    private void UpdateFacing(float xDir)
    {
        if (Mathf.Abs(xDir) < 0.1f) return;
        transform.localScale = new Vector3(xDir > 0 ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, bodyRadius);
    }
}
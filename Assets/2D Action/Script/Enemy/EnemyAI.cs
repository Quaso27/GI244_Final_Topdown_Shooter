using UnityEngine;

public class SmartEnemyAI : EnemyBase
{
    [Header("AI Movement Settings")]
    public float moveSpeed = 3.5f;
    public float detectionRadius = 2.5f; // ระยะตรวจจับสิ่งกีดขวาง
    public float bodyRadius = 0.4f;      // รัศมีตัวมอนสเตอร์ ป้องกันการเดินเบียดมุมกำแพง
    public LayerMask obstacleLayer;

    [Header("AI Behavior Parameters")]
    [Range(0, 1)] public float momentumWeight = 0.2f; // น้ำหนักทิศทางเดิม ช่วยลดการสั่นกึกๆ
    public float dangerWeight = 1.5f;                // น้ำหนักแรงผลักให้ออกห่างจากกำแพง

    private Vector2 lastDirection;

    // ทิศทางมาตรฐาน 8 ทิศรอบตัว
    private readonly Vector2[] directions = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right,
        new Vector2(1,1).normalized, new Vector2(1,-1).normalized,
        new Vector2(-1,1).normalized, new Vector2(-1,-1).normalized
    };

    protected override void Start()
    {
        // เรียก Start ของ EnemyBase เพื่อเปิดระบบสแกนหา P1 / P2 คนที่ใกล้ที่สุด
        base.Start();

        // ล็อคการหมุนของ Rigidbody
        if (rb != null) rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        // เช็คสถานะตัวแปรจากคลาสแม่ (ถ้าตาย, ติดสตัน, หรือไม่มีผู้เล่นเหลืออยู่ ให้หยุดเดิน)
        if (isDead || isStunned || playerTarget == null) return;

        Vector2 bestDir = CalculateContextSteering();

        // ควบคุมการเคลื่อนที่ผ่าน Rigidbody ด้วยความเร็วคงที่นุ่มนวล
        rb.linearVelocity = bestDir * moveSpeed;

        // หันหน้าตามทิศทางเคลื่อนที่จริง (Flip Sprite) โดยไม่ทำลายสเกลดั้งเดิม
        UpdateFacing(rb.linearVelocity.x);
    }

    Vector2 CalculateContextSteering()
    {
        float[] interests = new float[8];
        float[] dangers = new float[8];

        // อ้างอิงทิศทางเข้าหาอัศวินจาก playerTarget ของ EnemyBase ล่าสุด
        Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;

        for (int i = 0; i < 8; i++)
        {
            // 1. คำนว0ณระดับความสนใจ (Interest) ในแต่ละทิศทาง
            float dotProduct = Vector2.Dot(directions[i], directionToPlayer);

            // คำนวณค่า Momentum เพื่อให้ AI สนใจทิศทางเดิมเล็กน้อย ช่วยให้ไถขอบกำแพงพริ้วขึ้น
            float momentum = Vector2.Dot(directions[i], lastDirection) * momentumWeight;

            interests[i] = Mathf.Max(0, dotProduct + momentum);

            // 2. คำนวณอันตราย (Danger) จากสิ่งกีดขวางรอบตัวด้วย CircleCast
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, bodyRadius, directions[i], detectionRadius, obstacleLayer);

            if (hit.collider != null)
            {
                // ยิ่งใกล้สิ่งกีดขวางมาก คะแนนอันตรายยิ่งพุ่งสูงขึ้นแบบก้าวกระโดด
                float distanceRatio = hit.distance / detectionRadius;
                dangers[i] = (1.0f - distanceRatio) * dangerWeight;
            }
        }

        // 3. หักลบค่าและรวมผลลัพธ์เวกเตอร์
        Vector2 outputDirection = Vector2.zero;
        for (int i = 0; i < 8; i++)
        {
            interests[i] = Mathf.Clamp01(interests[i] - dangers[i]);
            outputDirection += directions[i] * interests[i];
        }

        // แก้ไขบั๊กทิศทางเป็นศูนย์ (ป้องกันปัญหาเวกเตอร์ NaN หายไปจากจอ)
        if (outputDirection.sqrMagnitude > 0.01f)
        {
            outputDirection.Normalize();
            lastDirection = outputDirection; // เก็บไว้คำนวณ Momentum ในเฟรมถัดไป
            return outputDirection;
        }

        return Vector2.zero; // ถ้าไม่มีทางไปจริงๆ ให้ยืนนิ่งๆ รอ
    }

    private void UpdateFacing(float xVelocity)
    {
        // ถ้าแทบไม่ขยับในแนวแกน X ไม่ต้องเปลี่ยนทิศทางการมอง
        if (Mathf.Abs(xVelocity) < 0.05f) return;

        // พลิกหน้าโดยอิงตามตัวแปรเดิม ไม่ให้โมเดลยืดหรือหดผิดเพี้ยน
        float targetScaleX = xVelocity > 0 ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(targetScaleX, transform.localScale.y, transform.localScale.z);
    }

    private void OnDrawGizmosSelected()
    {
        // วาดเส้นรัศมีช่วยดูพฤติกรรม AI ในหน้า Scene View
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, bodyRadius);
    }
}
using UnityEngine;
using System.Collections;

public class EnemyBat : EnemyBase
{
    [Header("Movement Settings")]
    public float flySpeed = 3f;
    public float dashSpeed = 12f;
    public float detectionRange = 5f;
    public float dashDuration = 0.4f;
    public float dashCooldown = 2.5f;

    [Header("Smart AI Steering")]
    public float obstacleDetectionRadius = 2.5f; // ระยะเรดาร์มองกำแพง
    public float bodyRadius = 0.4f;             // รัศมีตัวค้างคาว (ป้องกันเบียดมุม)
    public LayerMask obstacleLayer;
    [Range(0, 1)] public float momentumWeight = 0.2f; // ช่วยให้บินเลาะกำแพงพริ้วขึ้น

    private Transform player;
    private bool isDashing = false;
    private float nextDashTime;
    private Vector2 lastDirection;

    // ทิศทางเรดาร์ 8 ทิศ
    private readonly Vector2[] directions = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right,
        new Vector2(1,1).normalized, new Vector2(1,-1).normalized,
        new Vector2(-1,1).normalized, new Vector2(-1,-1).normalized
    };

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (rb != null) rb.freezeRotation = true;
    }

    private void Update()
    {
        if (isDead || player == null || isDashing) return;

        // เช็คระยะเพื่อพุ่งโจมตี (Dash)
        if (Vector2.Distance(transform.position, player.position) <= detectionRange && Time.time >= nextDashTime)
        {
            StartCoroutine(SwoopDash());
        }
    }

    private void FixedUpdate()
    {
        if (isDead || player == null || isDashing || isStunned) return;

        // --- ใช้ระบบ Smart AI ในการบินปกติ ---
        Vector2 smartDir = CalculateContextSteering();
        rb.linearVelocity = smartDir * flySpeed;

        UpdateFacing(rb.linearVelocity.x);
    }

    // ระบบ AI คำนวณทิศทางหลบหลีก (Context Steering)
    Vector2 CalculateContextSteering()
    {
        float[] interests = new float[8];
        float[] dangers = new float[8];
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        for (int i = 0; i < 8; i++)
        {
            // 1. Interest: คะแนนความอยากไปหาผู้เล่น
            float d = Vector2.Dot(directions[i], directionToPlayer);

            // เพิ่ม Momentum: ให้คะแนนทิศทางเดิมเล็กน้อย ลดอาการสั่นและช่วยไถกำแพง
            float momentum = Vector2.Dot(directions[i], lastDirection) * momentumWeight;
            interests[i] = Mathf.Max(0, d + momentum);

            // 2. Danger: ตรวจสอบสิ่งกีดขวางด้วย CircleCast (เช็คตามขนาดตัว)
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, bodyRadius, directions[i], obstacleDetectionRadius, obstacleLayer);
            if (hit.collider != null)
            {
                // ยิ่งใกล้กำแพง คะแนนอันตรายยิ่งสูง
                dangers[i] = 1.0f - (hit.distance / obstacleDetectionRadius);
            }
        }

        // 3. หักลบ Interest ด้วย Danger และรวมทิศทาง
        Vector2 outputDirection = Vector2.zero;
        for (int i = 0; i < 8; i++)
        {
            interests[i] = Mathf.Clamp01(interests[i] - dangers[i]);
            outputDirection += directions[i] * interests[i];
        }

        // เก็บข้อมูลทิศทางล่าสุดไว้ใช้ใน Frame ถัดไป
        if (outputDirection.sqrMagnitude > 0.01f)
        {
            lastDirection = outputDirection.normalized;
        }

        // ป้องกันค่า NaN ถ้าไม่มีทางไปเลยให้ส่ง Vector2.zero
        return outputDirection.sqrMagnitude > 0.01f ? outputDirection.normalized : Vector2.zero;
    }

    IEnumerator SwoopDash()
    {
        isDashing = true;

        // หยุดรอจังหวะก่อนพุ่ง
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.2f);

        if (isDead || player == null) { isDashing = false; yield break; }

        // ตอนพุ่ง (Dash) ให้พุ่งตรงๆ ใส่ผู้เล่นเลย (เน้นความดุดัน)
        Vector2 targetDir = (player.position - transform.position).normalized;
        rb.linearVelocity = targetDir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        // ชะลอความเร็วหลังพุ่ง (ค่อยๆ หยุด)
        float elapsed = 0f;
        Vector2 currentVel = rb.linearVelocity;
        while (elapsed < 0.2f)
        {
            if (isDead) break;
            rb.linearVelocity = Vector2.Lerp(currentVel, Vector2.zero, elapsed / 0.2f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        nextDashTime = Time.time + dashCooldown;
    }

    private void UpdateFacing(float xDir)
    {
        if (Mathf.Abs(xDir) < 0.1f) return;
        // ปรับการ Flip ให้เข้ากับ EnemyBase ของคุณ
        transform.localScale = new Vector3(xDir > 0 ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }

    // วาด Debug ในหน้า Scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, obstacleDetectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, bodyRadius);
    }
}
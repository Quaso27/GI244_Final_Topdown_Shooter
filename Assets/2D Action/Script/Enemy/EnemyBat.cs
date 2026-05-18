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
    public float obstacleDetectionRadius = 2.5f;
    public float bodyRadius = 0.4f;
    public LayerMask obstacleLayer;
    [Range(0, 1)] public float momentumWeight = 0.2f;

    private bool isDashing = false;
    private float nextDashTime;
    private Vector2 lastDirection;

    private readonly Vector2[] directions = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right,
        new Vector2(1,1).normalized, new Vector2(1,-1).normalized,
        new Vector2(-1,1).normalized, new Vector2(-1,-1).normalized
    };

    protected override void Start()
    {
        base.Start();

        if (rb != null) rb.freezeRotation = true;
    }

    protected override void Update()
    {
        base.Update();

        if (isDead || playerTarget == null || isDashing) return;

        if (Vector2.Distance(transform.position, playerTarget.position) <= detectionRange && Time.time >= nextDashTime)
        {
            StartCoroutine(SwoopDash());
        }
    }

    private void FixedUpdate()
    {
        if (isDead || playerTarget == null || isDashing || isStunned) return;

        Vector2 smartDir = CalculateContextSteering();
        rb.linearVelocity = smartDir * flySpeed;

        UpdateFacing(rb.linearVelocity.x);
    }

    Vector2 CalculateContextSteering()
    {
        float[] interests = new float[8];
        float[] dangers = new float[8];
        Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;

        for (int i = 0; i < 8; i++)
        {
            float d = Vector2.Dot(directions[i], directionToPlayer);

            float momentum = Vector2.Dot(directions[i], lastDirection) * momentumWeight;
            interests[i] = Mathf.Max(0, d + momentum);

            RaycastHit2D hit = Physics2D.CircleCast(transform.position, bodyRadius, directions[i], obstacleDetectionRadius, obstacleLayer);
            if (hit.collider != null)
            {
                dangers[i] = 1.0f - (hit.distance / obstacleDetectionRadius);
            }
        }

        Vector2 outputDirection = Vector2.zero;
        for (int i = 0; i < 8; i++)
        {
            interests[i] = Mathf.Clamp01(interests[i] - dangers[i]);
            outputDirection += directions[i] * interests[i];
        }

        if (outputDirection.sqrMagnitude > 0.01f)
        {
            lastDirection = outputDirection.normalized;
        }

        return outputDirection.sqrMagnitude > 0.01f ? outputDirection.normalized : Vector2.zero;
    }

    IEnumerator SwoopDash()
    {
        isDashing = true;

        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.2f);

        if (isDead || playerTarget == null) { isDashing = false; yield break; }

        Vector2 targetDir = (playerTarget.position - transform.position).normalized;
        rb.linearVelocity = targetDir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

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
        transform.localScale = new Vector3(xDir > 0 ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, obstacleDetectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, bodyRadius);
    }
}
using UnityEngine;

public class EnemySlime : EnemyBase
{
    [Header("AI Movement Settings")]
    public float moveSpeed = 2f;
    public float detectionRadius = 1.5f;
    public float bodyRadius = 0.3f;
    public LayerMask obstacleLayer;

    [Range(0, 1)] public float momentumWeight = 0.15f;

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

    void FixedUpdate()
    {
        if (isDead || playerTarget == null || isStunned) return;

        Vector2 smartDir = CalculateContextSteering();
        rb.linearVelocity = smartDir * moveSpeed;

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

            RaycastHit2D hit = Physics2D.CircleCast(transform.position, bodyRadius, directions[i], detectionRadius, obstacleLayer);
            if (hit.collider != null)
            {
                dangers[i] = 1.0f - (hit.distance / detectionRadius);
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
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HomingBullet : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 15f;
    public float rotateSpeed = 1500f;
    public float detectionRadius = 25f;

    [Header("Combat Settings")]
    public int damage = 1;
    public int maxChainKills = 3;
    public int maxWallBounces = 2;

    private int currentKills = 0;
    private int currentWallBounces = 0;
    private Rigidbody2D rb;
    private Transform target;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.angularVelocity = 0f;
        target = FindClosestEnemy();
        rb.linearVelocity = transform.up * speed;
        Destroy(gameObject, 6f);
    }

    void FixedUpdate()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            target = FindClosestEnemy();
        }

        if (target != null)
        {
            Vector2 direction = (Vector2)target.position - rb.position;
            direction.Normalize();

            float rotateAmount = Vector3.Cross(direction, transform.up).z;

            rb.angularVelocity = -rotateAmount * rotateSpeed;
        }
        else
        {
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, 0, Time.fixedDeltaTime * 5f);
        }
        rb.linearVelocity = transform.up * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.gameObject.TryGetComponent<EnemyBase>(out EnemyBase enemy))
            {
                enemy.TakeDamage(damage);
                currentKills++;

                if (currentKills >= maxChainKills)
                {
                    Destroy(gameObject);
                }
                else
                {
                    target = FindClosestEnemy(); 
                    if (target != null)
                    {
                        Vector2 nextDir = (Vector2)target.position - rb.position;
                        float angle = Mathf.Atan2(nextDir.y, nextDir.x) * Mathf.Rad2Deg - 90f;
                        rb.rotation = angle;
                    }
                }
            }
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            if (currentWallBounces < maxWallBounces)
            {
                currentWallBounces++;
                Vector2 reflectDir = Vector2.Reflect(transform.up, collision.contacts[0].normal);
                rb.rotation = Mathf.Atan2(reflectDir.y, reflectDir.x) * Mathf.Rad2Deg - 90f;
                target = null;
            }
            else { Destroy(gameObject); }
        }
    }

    Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        foreach (GameObject go in enemies)
        {
            if (!go.activeInHierarchy) continue;

            float curDistSq = (go.transform.position - transform.position).sqrMagnitude;
            if (curDistSq < distance && curDistSq < (detectionRadius * detectionRadius))
            {
                closest = go;
                distance = curDistSq;
            }
        }
        return closest != null ? closest.transform : null;
    }
}
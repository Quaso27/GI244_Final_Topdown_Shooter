using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HomingBullet : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 15f;
    public float rotateSpeed = 1500f; // เพิ่มให้สูงขึ้นเพื่อให้หันกลับมาหาเป้าได้ไว
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
        // ค้นหาเป้าหมาย
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            target = FindClosestEnemy();
        }

        if (target != null)
        {
            Vector2 direction = (Vector2)target.position - rb.position;
            direction.Normalize();

            // ใช้การคำนวณ Cross Product เพื่อหาแรงบิด
            float rotateAmount = Vector3.Cross(direction, transform.up).z;

            // เพิ่มความแรงในการหมุน (Steering)
            rb.angularVelocity = -rotateAmount * rotateSpeed;
        }
        else
        {
            // ถ้าไม่มีเป้า ให้ค่อยๆ ลดความเร็วมุมหมุนลง ไม่ให้หมุนเคว้ง
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, 0, Time.fixedDeltaTime * 5f);
        }

        // บังคับความเร็วให้คงที่ตลอดเวลา เพื่อไม่ให้แรงชนทำความเร็วตก
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
                    // --- หัวใจสำคัญของการล็อคเป้าใหม่ ---
                    target = FindClosestEnemy(); // หาตัวใหม่ทันที
                    if (target != null)
                    {
                        // หันหน้าไปหาเป้าหมายใหม่ทันที "แบบฉับพลัน" เพื่อแก้แรงกระเด็น
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
            // เช็คเฉพาะตัวที่ยังมีชีวิตอยู่ (Active)
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
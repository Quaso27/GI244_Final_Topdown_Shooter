using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 20f;

    [Header("Orientation Fix")]
    [Tooltip("ถ้ากระสุนออกมาผิดด้าน ให้ลองปรับมุมตรงนี้ เช่น 90 หรือ -90")]
    public float spriteRotationOffset = 0f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = transform.up * speed;

        transform.Rotate(0, 0, spriteRotationOffset);

        Destroy(gameObject, 2f);
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Enemy"))
        {
            EnemyBase enemy = hitInfo.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(1); 
            }
            Destroy(gameObject); 
        }

        if (hitInfo.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Base Stats")]
    public int health = 3;
    public int contactDamage = 1;
    public int scoreValue = 10;

    [Header("Audio & Visuals")]
    public AudioClip deathSound;
    [Range(0f, 1f)] public float deathVolume = 1.0f;

    protected bool isDead = false;
    protected bool isStunned = false;
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Collider2D col;
    protected EnemySpawner spawner;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    protected virtual void Start()
    {
        spawner = Object.FindFirstObjectByType<EnemySpawner>();
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        HandleDamage(collision.gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        HandleDamage(other.gameObject);
    }

    private void HandleDamage(GameObject target)
    {
        if (isDead) return;

        if (target.CompareTag("Player"))
        {
            var player = target.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(contactDamage);
                Debug.Log(gameObject.name + " hit Player!");
            }
        }
    }

    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;
        health -= amount;
        if (health <= 0) Die();
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        // --- ส่วนที่เพิ่มเข้ามาเพื่อให้ไอเทมดร็อป (image_4b315a.png) ---
        ItemDropper dropper = GetComponent<ItemDropper>();
        if (dropper != null)
        {
            dropper.OnEnemyDeath();
        }
        // --------------------------------------------------

        if (spawner != null) spawner.RecordEnemyDeath();

        if (GameManager.instance != null)
        {
            GameManager.instance.AddScore(scoreValue);
            GameManager.instance.AddKills();
        }

        PlayDeathEffects();

        if (anim != null) anim.SetTrigger("Die");

        if (col != null) col.enabled = false;

        if (rb != null)
        {
            rb.linearDamping = 5f;
            rb.linearVelocity = Vector2.zero; // หยุดแรงส่งจากการเดินปกติ
        }

        Destroy(gameObject, 0.6f);
    }

    private void PlayDeathEffects()
    {
        if (deathSound == null) return;

        // เรียกใช้ผ่าน SoundManager เพื่อให้เสียงเบาลงตาม Slider (image_4bac7b.png)
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlaySFX(deathSound, deathVolume);
        }
    }
}
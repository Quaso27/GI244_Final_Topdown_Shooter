using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Base Stats")]
    public int health = 3;
    public int contactDamage = 1;
    public int scoreValue = 10;

    [Header("Audio & Visuals")]
    public AudioClip deathSound;
    public AudioClip attackHitSound; 
    [Range(0f, 1f)] public float deathVolume = 1.0f;

    protected Transform playerTarget;

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

        FindClosestPlayer();
    }

    protected virtual void Update()
    {
        if (isDead) return;

        FindClosestPlayer();
    }

    protected void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = Mathf.Infinity;
        Transform target = null;

        foreach (GameObject p in players)
        {
            if (p != null && p.gameObject.activeInHierarchy)
            {
                float distance = Vector2.Distance(transform.position, p.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    target = p.transform;
                }
            }
        }

        playerTarget = target;
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
            var playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
                Debug.Log(gameObject.name + " hit Player! Damage: " + contactDamage);

                if (CameraShake.instance != null)
                {
                    CameraShake.instance.Shake(0.15f, 0.25f);
                }

                if (DamageFlashEffect.instance != null)
                {
                    DamageFlashEffect.instance.StartFlash();
                }

                if (attackHitSound != null && SoundManager.instance != null)
                {
                    SoundManager.instance.PlaySFX(attackHitSound, deathVolume);
                }
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

        ItemDropper dropper = GetComponent<ItemDropper>();
        if (dropper != null)
        {
            dropper.OnEnemyDeath();
        }

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
            rb.linearVelocity = Vector2.zero;
        }

        Destroy(gameObject, 0.6f);
    }

    private void PlayDeathEffects()
    {
        if (deathSound == null) return;

        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlaySFX(deathSound, deathVolume);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSlowEnemy : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private float slowAmount = 2f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private float despawnTime = 10f;

    private bool _isCollected = false;

    private void Start()
    {
        Invoke(nameof(SelfDestroy), despawnTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !_isCollected)
        {
            _isCollected = true;
            CancelInvoke(nameof(SelfDestroy));

            if (TryGetComponent<SpriteRenderer>(out var sr)) sr.enabled = false;
            if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;

            StartCoroutine(SlowRoutine());
        }
    }

    private void SelfDestroy()
    {
        if (!_isCollected) Destroy(gameObject);
    }

    private IEnumerator SlowRoutine()
    {
        List<EnemySlime> slowedSlimes = new List<EnemySlime>();

        EnemyBase[] enemies = Object.FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);

        foreach (EnemyBase enemy in enemies)
        {
            if (enemy is EnemySlime slime && slime != null)
            {
                slime.moveSpeed -= slowAmount;
                if (slime.moveSpeed < 0.5f) slime.moveSpeed = 0.5f;

                slowedSlimes.Add(slime);
            }
        }
        yield return new WaitForSeconds(duration);

        foreach (EnemySlime slime in slowedSlimes)
        {
            if (slime != null) 
            {
                slime.moveSpeed += slowAmount;
            }
        }
        Destroy(gameObject);
    }
}
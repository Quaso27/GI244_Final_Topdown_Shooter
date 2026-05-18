using System.Collections;
using UnityEngine;

public class ItemSpeedBoost : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speedMultiplier = 5f;
    [SerializeField] private float duration = 3f;
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
            PlayerController pc = other.GetComponent<PlayerController>();

            if (pc != null)
            {
                ApplyCollectionEffect(pc);
            }
        }
    }

    private void ApplyCollectionEffect(PlayerController pc)
    {
        _isCollected = true;
        CancelInvoke(nameof(SelfDestroy));

        if (TryGetComponent<SpriteRenderer>(out var sr)) sr.enabled = false;
        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;

        StartCoroutine(SpeedBoostRoutine(pc));
    }

    private void SelfDestroy()
    {
        if (!_isCollected)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator SpeedBoostRoutine(PlayerController pc)
    {
        float boostAmount = speedMultiplier;
        pc.SetMoveSpeed(pc.GetMoveSpeed() + boostAmount);

        Debug.Log($"<color=green>Speed Up!</color> Current Speed: {pc.GetMoveSpeed()}");

        yield return new WaitForSecondsRealtime(duration);

        if (pc != null)
        {
            pc.SetMoveSpeed(pc.GetMoveSpeed() - boostAmount);
            Debug.Log($"<color=white>Speed Normal.</color> Current Speed: {pc.GetMoveSpeed()}");
        }
        Destroy(gameObject);
    }
}
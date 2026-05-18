using UnityEngine;

public class MagneticItem : MonoBehaviour
{
    [Header("Magnet Settings")]
    public float moveSpeed = 5f;        
    public float acceleration = 2f;    

    private Transform targetPlayer;
    private bool isBeingPulled = false;
    private float currentSpeed;

    void Start()
    {
        currentSpeed = moveSpeed;
    }

    void Update()
    {
        if (isBeingPulled && targetPlayer != null)
        {
            Vector3 direction = (targetPlayer.position - transform.position).normalized;

            currentSpeed += acceleration * Time.deltaTime;

            transform.position += direction * currentSpeed * Time.deltaTime;
        }
    }

    public void StartPull(Transform playerTransform)
    {
        if (!isBeingPulled)
        {
            targetPlayer = playerTransform;
            isBeingPulled = true;
        }
    }
}
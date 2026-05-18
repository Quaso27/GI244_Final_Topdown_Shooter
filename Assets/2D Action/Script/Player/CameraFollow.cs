using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player1;
    public Transform player2;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    void LateUpdate()
    {
        Vector3 targetPosition;
        bool isP1Active = player1 != null && player1.gameObject.activeInHierarchy;
        bool isP2Active = player2 != null && player2.gameObject.activeInHierarchy;

        if (isP1Active && isP2Active)
        {
            targetPosition = (player1.position + player2.position) / 2f;
        }
        else if (isP1Active)
        {
            targetPosition = player1.position;
        }
        else if (isP2Active)
        {
            targetPosition = player2.position;
        }
        else
        {
            return;
        }

        Vector3 desiredPosition = targetPosition + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}
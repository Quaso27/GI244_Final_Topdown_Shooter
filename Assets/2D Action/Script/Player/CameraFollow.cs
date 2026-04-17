using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;     
    public float smoothSpeed = 5f; 
    public Vector3 offset = new Vector3(0, 0, -10); 

    void LateUpdate()
    {
        if (target != null)
        {
            // คำนวณตำแหน่งที่กล้องควรอยู่ โดยไม่อิงกับค่า Rotation ของ Target
            Vector3 desiredPosition = target.position + offset;

        
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // อัปเดตตำแหน่งกล้อง
            transform.position = smoothedPosition;

            // บังคับให้กล้องหันหน้าตรงตลอดเวลา (Reset Rotation)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
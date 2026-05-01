using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaserBeam : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [Header("Visual Settings")]
    public float visualWidth = 0.4f;      // ความกว้างของภาพเลเซอร์ที่ผู้เล่นเห็น (ปรับให้เล็กลงเพื่อความคม)
    public float laserDuration = 0.2f;    // ระยะเวลาที่เลเซอร์แสดงผล
    public float growSpeed = 150f;        // ความเร็วในการพุ่งของเส้นเลเซอร์

    [Header("Combat Settings")]
    public float hitboxWidth = 0.4f;      // ความกว้างของพื้นที่การชน (แยกอิสระจากภาพ)
    public int damage = 20;

    // ลบ [public float maxRange] ออกเพื่อให้ระบบยึดตามค่าที่ส่งมาจาก PlayerController เป็นหลัก

    [Header("Visual Assets")]
    public Material laserMaterial;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) lineRenderer = gameObject.AddComponent<LineRenderer>();

        // ตั้งค่าเริ่มต้นให้โปร่งใสเพื่อป้องกันเส้นสีแดงแวบขึ้นมา
        lineRenderer.startColor = new Color(1, 1, 1, 0);
        lineRenderer.endColor = new Color(1, 1, 1, 0);
        lineRenderer.enabled = false;

        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;

        lineRenderer.startWidth = visualWidth;
        lineRenderer.endWidth = visualWidth;

        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.sortingOrder = 1000; // บังคับให้อยู่หน้าสุด

        if (laserMaterial != null)
        {
            lineRenderer.material = laserMaterial;
        }
    }

    /// <summary>
    /// ฟังก์ชันสั่งยิงเลเซอร์ โดยรับพารามิเตอร์ระยะทาง (range) มาจากผู้ส่ง
    /// </summary>
    public void Fire(Vector3 startPos, Vector3 direction, float range)
    {
        // ยึดตามค่า range ที่ส่งมาจาก PlayerController ตรงๆ
        float finalDistance = range;
        Vector3 endPos = startPos + (direction.normalized * finalDistance);

        // --- ระบบ Hitbox ---
        // ใช้ LayerMask จะช่วยให้ประสิทธิภาพดีขึ้น (ถ้ามี) แต่ตอนนี้ใช้ Tag เช็คเหมือนเดิม
        RaycastHit2D[] hits = Physics2D.CircleCastAll(startPos, hitboxWidth / 2f, direction, finalDistance);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                // ส่งดาเมจไปยังศัตรู
                hit.collider.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
        }

        StopAllCoroutines();
        StartCoroutine(AnimateLaser(startPos, endPos));
    }

    private IEnumerator AnimateLaser(Vector3 start, Vector3 targetEnd)
    {
        // คืนค่าสีกลับเป็นทึบแสงเมื่อเริ่มยิง
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.enabled = true;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, start);

        float currentDist = 0;
        float maxDist = Vector3.Distance(start, targetEnd);
        Vector3 dir = (targetEnd - start).normalized;

        // ขั้นตอนการพุ่งออกไป (Laser Growth)
        while (currentDist < maxDist)
        {
            currentDist += growSpeed * Time.deltaTime;
            currentDist = Mathf.Min(currentDist, maxDist);
            lineRenderer.SetPosition(1, start + (dir * currentDist));
            yield return null;
        }

        // ค้างไว้ตามระยะเวลาที่กำหนดแล้วค่อยจางหาย (Fade Out)
        float elapsed = 0;
        while (elapsed < laserDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / laserDuration);

            Color c = lineRenderer.startColor; // ใช้สีปัจจุบัน
            c.a = alpha;
            lineRenderer.startColor = c;
            lineRenderer.endColor = c;
            yield return null;
        }

        lineRenderer.enabled = false;
        Destroy(gameObject);
    }

    // วาด Gizmos เพื่อตรวจสอบระยะและขนาด Hitbox ในหน้า Scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hitboxWidth / 2f);
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaserBeam : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [Header("Visual Settings")]
    public float visualWidth = 0.4f;      
    public float laserDuration = 0.2f;   
    public float growSpeed = 150f;        

    [Header("Combat Settings")]
    public float hitboxWidth = 0.4f;      
    public int damage = 20;

    [Header("Visual Assets")]
    public Material laserMaterial;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null) lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.startColor = new Color(1, 1, 1, 0);
        lineRenderer.endColor = new Color(1, 1, 1, 0);
        lineRenderer.enabled = false;

        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2;

        lineRenderer.startWidth = visualWidth;
        lineRenderer.endWidth = visualWidth;

        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.sortingOrder = 1000; 

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
        float finalDistance = range;
        Vector3 endPos = startPos + (direction.normalized * finalDistance);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(startPos, hitboxWidth / 2f, direction, finalDistance);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                hit.collider.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
        }

        StopAllCoroutines();
        StartCoroutine(AnimateLaser(startPos, endPos));
    }

    private IEnumerator AnimateLaser(Vector3 start, Vector3 targetEnd)
    {
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.enabled = true;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, start);

        float currentDist = 0;
        float maxDist = Vector3.Distance(start, targetEnd);
        Vector3 dir = (targetEnd - start).normalized;

        while (currentDist < maxDist)
        {
            currentDist += growSpeed * Time.deltaTime;
            currentDist = Mathf.Min(currentDist, maxDist);
            lineRenderer.SetPosition(1, start + (dir * currentDist));
            yield return null;
        }
        float elapsed = 0;
        while (elapsed < laserDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / laserDuration);

            Color c = lineRenderer.startColor; 
            c.a = alpha;
            lineRenderer.startColor = c;
            lineRenderer.endColor = c;
            yield return null;
        }

        lineRenderer.enabled = false;
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hitboxWidth / 2f);
    }
}
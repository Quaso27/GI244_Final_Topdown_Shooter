using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    public float speed = 15f;
    public float rotateSpeed = 500f;
    private Rigidbody2D rb;
    private Transform target;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        target = FindClosestEnemy();

        // ถ้าไม่เจอศัตรูเลย ให้ทำลายตัวเองทิ้ง 
        if (target == null) Destroy(gameObject, 2f);

        Destroy(gameObject, 4f); // ป้องกันกระสุนบินติด loop
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            Vector2 direction = (Vector2)target.position - rb.position;
            direction.Normalize();

            // คำนวณแรงบิดเพื่อให้หัวกระสุนหันไปหาเป้าหมาย
            float rotateAmount = Vector3.Cross(direction, transform.up).z;
            rb.angularVelocity = -rotateAmount * rotateSpeed;

            // พุ่งไปข้างหน้าตามทิศของตัวเอง
            rb.linearVelocity = transform.up * speed;
        }
        else
        {
            // ถ้าตายระหว่างทาง ให้พุ่งตรงไปเฉยๆ
            rb.linearVelocity = transform.up * speed;
        }
    }

    Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (GameObject go in enemies)
        {
            float curDistance = (go.transform.position - position).sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest != null ? closest.transform : null;
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Enemy"))
        {
            GameManager.instance.AddScore(10);
            Destroy(hitInfo.gameObject); 
            Destroy(gameObject);        
        }
    }
}
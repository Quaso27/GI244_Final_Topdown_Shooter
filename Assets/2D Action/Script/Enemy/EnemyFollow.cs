using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public float speed = 3f;
    private Transform player;

    public GameObject[] itemsToDrop;
    public float dropChance = 10f;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            Vector2 dir = player.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.instance.TakeDamage(1);
            Destroy(gameObject);
        }
    }

    private void OnDestroy() // แก้ไขเพื่อให้แต้ม Kills ขึ้น
    {
        if (!gameObject.scene.isLoaded) return;

        RollItemDrop();
    }

    void RollItemDrop()
    {
        if (Random.Range(0f, 100f) <= dropChance && itemsToDrop.Length > 0)
        {
            int randomIndex = Random.Range(0, itemsToDrop.Length);
            Instantiate(itemsToDrop[randomIndex], transform.position, Quaternion.identity);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 mousePos;
    public Camera cam;

    public GameObject bulletPrefab;
    public GameObject homingBulletPrefab;
    public Transform firePoint;

    public float skillCooldown = 5f;
    private float cooldownTimer = 0f;

    public Image cooldownImage;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        // รับ Input จาก WASD หรือลูกศร
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        //รับตำแหน่งเมาส์บนจอ
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownImage != null && skillCooldown > 0) 
            {
                cooldownImage.fillAmount = cooldownTimer / skillCooldown;
            }
        }
        else if (cooldownImage != null)
        {
            {
                cooldownImage.fillAmount = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && cooldownTimer <= 0)
        {
            UseHomingSkill();
        }    
        else if (Input.GetButtonDown("Fire1"))
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }

    void UseHomingSkill()
    {
        Instantiate(homingBulletPrefab, firePoint.position, firePoint.rotation);

        //นับ cd
        cooldownTimer = skillCooldown;
        Debug.Log("Skill Used! Cooldown started.");
    }

    void FixedUpdate()
    {
        // การเคลื่อนที่โดยใช้ Physics 
        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);

        //aiming logic
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
    }
}
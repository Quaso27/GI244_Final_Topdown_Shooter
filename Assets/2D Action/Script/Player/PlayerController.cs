using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public Camera cam;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    [Header("Shooting & Skills")]
    public GameObject bulletPrefab;
    public GameObject homingBulletPrefab;
    public Transform firePoint;
    public AudioClip shootSound;
    public AudioClip homingShootSound;
    public Image cooldownImage;
    public float skillCooldown = 5f;
    private float cooldownTimer = 0f;
    public bool hasHomingUpgrade = false;

    [Header("Augment: Charge Laser")]
    public bool hasChargeAugment = false;
    public GameObject laserPrefab;
    public float chargeTimeRequired = 1.0f;
    public float laserRange = 25f;

    [Header("Charge Audio & Effects")]
    public AudioClip chargeSound;
    public AudioClip laserShootSound;
    public AudioMixerGroup sfxGroup;
    private AudioSource chargeAudioSource;

    [Header("Charge Shake Settings")]
    [Range(0.01f, 0.5f)] public float shakeMagnitude = 0.15f;
    public float shakeSpeed = 60f;

    private float currentChargeTime = 0f;
    private bool isCharging = false;

    [Header("Magnet / Pickup Settings")]
    public float pickupRadius = 2.5f;
    private CircleCollider2D magnetCollider;

    [Header("HP Regeneration System")]
    public bool canRegen = true;
    public float regenRate = 1.0f;
    public int regenAmount = 1;
    public float regenDelay = 3.0f;
    private float lastDamageTime; // ใช้เก็บเวลาล่าสุดที่โดนโจมตี

    [Header("Audio (VFX/SFX)")]
    public AudioClip hurtSound;
    public AudioClip footstepClip;
    public float footstepDelay = 0.3f;
    private float footstepTimer;
    private bool isStepReady = false;

    [Header("Internal References")]
    [SerializeField] private Transform characterVisual;
    private Vector3 visualInitialPos;
    private PlayerHealth playerHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();

        if (cam == null) cam = Camera.main;
        rb.freezeRotation = true;

        chargeAudioSource = gameObject.AddComponent<AudioSource>();
        chargeAudioSource.playOnAwake = false;
        chargeAudioSource.loop = true;

        if (SoundManager.instance != null && SoundManager.instance.sfxSource != null)
            chargeAudioSource.outputAudioMixerGroup = SoundManager.instance.sfxSource.outputAudioMixerGroup;
        else if (sfxGroup != null)
            chargeAudioSource.outputAudioMixerGroup = sfxGroup;

        if (characterVisual == null && transform.childCount > 0)
            characterVisual = transform.GetChild(0);

        if (characterVisual != null)
            visualInitialPos = characterVisual.localPosition;

        SetupMagnet();

        // เริ่ม Coroutine เดียวทิ้งไว้เลย มันจะทำงานเมื่อ canRegen เป็น true เอง
        StartCoroutine(RegenerateHealthLoop());
    }

    void SetupMagnet()
    {
        GameObject magnetObj = new GameObject("PickupRadius");
        magnetObj.transform.SetParent(this.transform);
        magnetObj.transform.localPosition = Vector3.zero;

        magnetCollider = magnetObj.AddComponent<CircleCollider2D>();
        magnetCollider.isTrigger = true;
        magnetCollider.radius = 0f;
        magnetCollider.enabled = false;

        var detector = magnetObj.AddComponent<MagnetDetector>();
        detector.playerTransform = this.transform;
    }

    public void UpgradePickupRadius(float extraRadius)
    {
        if (magnetCollider != null)
        {
            magnetCollider.enabled = true;
            pickupRadius += extraRadius;
            magnetCollider.radius = pickupRadius;
        }
    }

    public void IncreaseMaxHealth(float percent)
    {
        if (playerHealth != null) playerHealth.IncreaseMaxHealth(percent);
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        if (Time.timeScale == 0f)
        {
            if (isCharging) StopChargeEffects();
            return;
        }

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        HandleCooldownUI();
        HandleFootstepSound();

        bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (Input.GetButtonDown("Fire1") && !isCharging && !isPointerOverUI) Shoot();

        if (Input.GetKeyDown(KeyCode.E) && cooldownTimer <= 0 && hasHomingUpgrade) UseHomingSkill();

        if (hasChargeAugment) HandleChargeAttack(isPointerOverUI);
    }

    void FixedUpdate()
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float currentSpeed = isCharging ? moveSpeed * 0.4f : moveSpeed;
        rb.linearVelocity = moveInput.normalized * currentSpeed;

        RotateTowardsMouse();
    }

    // --- ปรับปรุงระบบ HP REGEN (Single Loop Strategy) ---
    IEnumerator RegenerateHealthLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(regenRate);

            // เงื่อนไขการฟื้นฟู: 
            // 1. มีความสามารถ (canRegen)
            // 2. เลือดไม่เต็มและไม่ตาย
            // 3. เวลาปัจจุบันห่างจากเวลาที่โดนดาเมจล่าสุดมากกว่า regenDelay
            bool isOutsideDelay = Time.time - lastDamageTime > regenDelay;

            if (canRegen && isOutsideDelay && playerHealth != null)
            {
                if (playerHealth.currentHealth < playerHealth.maxHealth && playerHealth.currentHealth > 0)
                {
                    playerHealth.currentHealth += regenAmount;
                    playerHealth.currentHealth = Mathf.Min(playerHealth.currentHealth, (int)playerHealth.maxHealth);
                    playerHealth.UpdateHealthUI();
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;

        if (playerHealth != null) playerHealth.TakeDamage(damage);
        if (DamageFlashEffect.instance != null) DamageFlashEffect.instance.StartFlash();
        if (CameraShake.instance != null) CameraShake.instance.Shake(0.2f, 0.2f);
        if (hurtSound != null) PlaySound(hurtSound);

        // บันทึกเวลาที่โดนดาเมจล่าสุด เพื่อให้ Loop Regen ทำงานร่วมกับ delay ได้แม่นยำ
        lastDamageTime = Time.time;
    }

    // --- ระบบดั้งเดิม ---
    void RotateTowardsMouse()
    {
        Vector3 worldMousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = (Vector2)worldMousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
    }

    void Shoot()
    {
        if (firePoint != null && bulletPrefab != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            PlaySound(shootSound);
        }
    }

    void UseHomingSkill()
    {
        if (firePoint != null && homingBulletPrefab != null)
        {
            Instantiate(homingBulletPrefab, firePoint.position, firePoint.rotation);
            PlaySound(homingShootSound != null ? homingShootSound : shootSound);
            cooldownTimer = skillCooldown;
        }
    }

    void HandleChargeAttack(bool isOverUI)
    {
        if (Input.GetMouseButtonDown(1) && !isOverUI)
        {
            isCharging = true;
            currentChargeTime = 0f;
            if (chargeSound != null)
            {
                chargeAudioSource.clip = chargeSound;
                chargeAudioSource.pitch = 0.8f;
                chargeAudioSource.Play();
            }
        }

        if (isCharging && Input.GetMouseButton(1))
        {
            currentChargeTime += Time.deltaTime;
            float chargeRatio = Mathf.Clamp01(currentChargeTime / chargeTimeRequired);

            if (characterVisual != null)
            {
                float currentMagnitude = shakeMagnitude * chargeRatio;
                float offsetX = Mathf.Sin(Time.time * shakeSpeed) * currentMagnitude;
                float offsetY = Mathf.Cos(Time.time * shakeSpeed * 1.2f) * currentMagnitude;
                characterVisual.localPosition = visualInitialPos + new Vector3(offsetX, offsetY, 0);
            }

            if (chargeAudioSource.isPlaying)
                chargeAudioSource.pitch = 0.8f + (chargeRatio * 0.7f);
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (isCharging && currentChargeTime >= chargeTimeRequired)
            {
                FireLaserAtMouse();
                if (CameraShake.instance != null) CameraShake.instance.Shake(0.2f, 0.3f);
            }
            StopChargeEffects();
        }
    }

    void StopChargeEffects()
    {
        isCharging = false;
        currentChargeTime = 0f;
        if (chargeAudioSource != null && chargeAudioSource.isPlaying) chargeAudioSource.Stop();
        if (characterVisual != null) characterVisual.localPosition = visualInitialPos;
    }

    void FireLaserAtMouse()
    {
        if (laserPrefab != null && firePoint != null)
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector3 fireDirection = (mousePos - firePoint.position).normalized;

            GameObject laserObj = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);
            var beam = laserObj.GetComponent<LaserBeam>();
            if (beam != null) beam.Fire(firePoint.position, fireDirection, laserRange);

            PlaySound(laserShootSound != null ? laserShootSound : shootSound);
        }
    }

    void HandleFootstepSound()
    {
        if (footstepClip == null || moveInput.magnitude < 0.1f) { isStepReady = false; return; }
        if (!isStepReady) { footstepTimer = footstepDelay; isStepReady = true; }

        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0)
        {
            PlaySound(footstepClip);
            footstepTimer = footstepDelay;
        }
    }

    void HandleCooldownUI()
    {
        if (cooldownImage == null) return;
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            cooldownImage.fillAmount = cooldownTimer / skillCooldown;
        }
        else cooldownImage.fillAmount = 0;
    }

    void PlaySound(AudioClip clip)
    {
        if (SoundManager.instance != null && clip != null)
            SoundManager.instance.PlaySFX(clip);
    }

    public class MagnetDetector : MonoBehaviour
    {
        public Transform playerTransform;
        private void OnTriggerEnter2D(Collider2D other)
        {
            MagneticItem item = other.GetComponent<MagneticItem>();
            if (item != null) item.StartPull(playerTransform);
        }
    }
}
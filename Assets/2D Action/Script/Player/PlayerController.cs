using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [Header("Multiplayer Settings")]
    public bool isPlayerTwo = false; 

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Camera _cam;
    private Rigidbody2D _rb;
    private Vector2 _moveInput;

    [Header("Shooting & Skills")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private GameObject _homingBulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private AudioClip _shootSound;
    [SerializeField] private AudioClip _homingShootSound;
    [SerializeField] private Image _cooldownImage;
    [SerializeField] private float _skillCooldown = 5f;
    private float _cooldownTimer = 0f;
    public bool hasHomingUpgrade = false;

    [Header("Augment: Charge Laser")]
    public bool hasChargeAugment = false;
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private float _chargeTimeRequired = 1.0f;
    [SerializeField] private float _laserRange = 25f;

    [Header("Charge Audio & Effects")]
    [SerializeField] private AudioClip _chargeSound;
    [SerializeField] private AudioClip _laserShootSound;
    private AudioSource _chargeAudioSource;
    [Range(0.01f, 0.5f)][SerializeField] private float _shakeMagnitude = 0.15f;
    [SerializeField] private float _shakeSpeed = 60f;

    private float _currentChargeTime = 0f;
    private bool _isCharging = false;

    [Header("Magnet / Pickup Settings")]
    [SerializeField] private float _pickupRadius = 2.5f;
    private CircleCollider2D _magnetCollider;

    [Header("HP Regeneration System")]
    [SerializeField] private bool _canRegen = true;
    [SerializeField] private float _regenRate = 1.0f;
    [SerializeField] private int _regenAmount = 1;
    [SerializeField] private float _regenDelay = 3.0f;
    private float _lastDamageTime;

    [Header("Audio (VFX/SFX)")]
    [SerializeField] private AudioClip _hurtSound;
    [SerializeField] private AudioClip _footstepClip;
    [SerializeField] private float _footstepDelay = 0.3f;
    private float _footstepTimer;
    private bool _isStepReady = false;

    [Header("Internal References")]
    [SerializeField] private Transform _characterVisual;
    private Vector3 _visualInitialPos;
    private PlayerHealth _playerHealth;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerHealth = GetComponent<PlayerHealth>();

        if (_cam == null) _cam = Camera.main;
        _rb.freezeRotation = true;

        _chargeAudioSource = gameObject.AddComponent<AudioSource>();
        _chargeAudioSource.playOnAwake = false;
        _chargeAudioSource.loop = true;

        if (_characterVisual == null && transform.childCount > 0)
            _characterVisual = transform.GetChild(0);

        if (_characterVisual != null)
            _visualInitialPos = _characterVisual.localPosition;

        SetupMagnet();
        StartCoroutine(RegenerateHealthLoop());
    }

    void Update()
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;
        if (Time.timeScale == 0f)
        {
            if (_isCharging) StopChargeEffects();
            return;
        }

        // --- ระบบแยก Input P1 และ P2 ---
        if (!isPlayerTwo)
        {
            // Player 1: Keyboard + Mouse
            _moveInput.x = Input.GetAxisRaw("Horizontal");
            _moveInput.y = Input.GetAxisRaw("Vertical");

            bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

            if (Input.GetButtonDown("Fire1") && !_isCharging && !isPointerOverUI) Shoot();
            if (Input.GetKeyDown(KeyCode.E) && _cooldownTimer <= 0 && hasHomingUpgrade) UseHomingSkill();
            if (hasChargeAugment) HandleChargeAttack(isPointerOverUI);
        }
        else
        {
            // Player 2: Joystick / P2 Keys
            _moveInput.x = Input.GetAxisRaw("HorizontalP2");
            _moveInput.y = Input.GetAxisRaw("VerticalP2");

            if (Input.GetButtonDown("FireP2") && !_isCharging) Shoot();
            if (Input.GetButtonDown("SkillP2") && _cooldownTimer <= 0 && hasHomingUpgrade) UseHomingSkill();
            if (hasChargeAugment) HandleChargeAttackP2(); 
        }

        HandleCooldownUI();
        HandleFootstepSound();
    }

    void FixedUpdate()
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        float currentSpeed = _isCharging ? _moveSpeed * 0.4f : _moveSpeed;
        _rb.linearVelocity = _moveInput.normalized * currentSpeed;

        RotateTowardsInput(); 
    }

    // --- แก้ไขระบบหมุนตัวละคร ---
    void RotateTowardsInput()
    {
        if (!isPlayerTwo)
        {
            if (_cam != null)
            {
                Vector3 worldMousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
                Vector2 lookDir = (Vector2)worldMousePos - _rb.position;
                float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
                _rb.rotation = angle;
            }
            else
            {
                _cam = Camera.main;
            }
        }
        else
        {
            float aimX = Input.GetAxis("AimXP2");
            float aimY = Input.GetAxis("AimYP2");

            if (Mathf.Abs(aimX) > 0.1f || Mathf.Abs(aimY) > 0.1f)
            {
                float angle = Mathf.Atan2(aimX, -aimY) * Mathf.Rad2Deg;
                _rb.rotation = angle;
            }
        }
    }

    // --- เพิ่มระบบชาร์จสำหรับ P2 ---
    void HandleChargeAttackP2()
    {
        if (Input.GetButtonDown("ChargeP2"))
        {
            _isCharging = true;
            _currentChargeTime = 0f;
            if (_chargeSound != null)
            {
                _chargeAudioSource.clip = _chargeSound;
                _chargeAudioSource.pitch = 0.8f;
                _chargeAudioSource.Play();
            }
        }

        if (_isCharging && Input.GetButton("ChargeP2"))
        {
            _currentChargeTime += Time.deltaTime;
            float chargeRatio = Mathf.Clamp01(_currentChargeTime / _chargeTimeRequired);

            if (_characterVisual != null)
            {
                float currentMagnitude = _shakeMagnitude * chargeRatio;
                float offsetX = Mathf.Sin(Time.time * _shakeSpeed) * currentMagnitude;
                float offsetY = Mathf.Cos(Time.time * _shakeSpeed * 1.2f) * currentMagnitude;
                _characterVisual.localPosition = _visualInitialPos + new Vector3(offsetX, offsetY, 0);
            }

            if (_chargeAudioSource.isPlaying)
                _chargeAudioSource.pitch = 0.8f + (chargeRatio * 0.7f);
        }

        if (Input.GetButtonUp("ChargeP2"))
        {
            if (_isCharging && _currentChargeTime >= _chargeTimeRequired)
            {
                FireLaserP2(); // ยิงเลเซอร์แบบไม่ต้องใช้เมาส์
                if (CameraShake.instance != null) CameraShake.instance.Shake(0.2f, 0.3f);
            }
            StopChargeEffects();
        }
    }

    void FireLaserP2()
    {
        if (_laserPrefab != null && _firePoint != null)
        {
            // P2 ยิงไปตามทิศที่ตัวละครหันหน้าอยู่ (transform.up)
            GameObject laserObj = Instantiate(_laserPrefab, _firePoint.position, Quaternion.identity);
            var beam = laserObj.GetComponent<LaserBeam>();
            if (beam != null) beam.Fire(_firePoint.position, transform.up, _laserRange);

            PlaySound(_laserShootSound != null ? _laserShootSound : _shootSound);
        }
    }

    // --------------------------------------------------
    // ระบบอื่นๆ คงเดิมทั้งหมด (Magnet, HP Regen, Shoot, etc.)
    // --------------------------------------------------

    void SetupMagnet()
    {
        GameObject magnetObj = new GameObject("PickupRadius");
        magnetObj.transform.SetParent(this.transform);
        magnetObj.transform.localPosition = Vector3.zero;

        _magnetCollider = magnetObj.AddComponent<CircleCollider2D>();
        _magnetCollider.isTrigger = true;
        _magnetCollider.radius = 0f;
        _magnetCollider.enabled = false;

        var detector = magnetObj.AddComponent<MagnetDetector>();
        detector.playerTransform = this.transform;
    }

    public void UpgradePickupRadius(float extraRadius)
    {
        if (_magnetCollider != null)
        {
            _magnetCollider.enabled = true;
            _pickupRadius += extraRadius;
            _magnetCollider.radius = _pickupRadius;
        }
    }

    IEnumerator RegenerateHealthLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_regenRate);
            bool isOutsideDelay = Time.time - _lastDamageTime > _regenDelay;

            if (_canRegen && isOutsideDelay && _playerHealth != null)
            {
                _playerHealth.Heal(_regenAmount);
            }
        }
    }

    public void SetRegenActive(bool isActive) { _canRegen = isActive; }
    public void SetRegenAmount(int amount) { _regenAmount = amount; }
    public float GetMoveSpeed() { return _moveSpeed; }
    public void SetMoveSpeed(float newSpeed) { _moveSpeed = newSpeed; }

    public void TakeDamage(float damage)
    {
        if (GameManager.instance != null && GameManager.instance.isGameOver) return;
        if (_playerHealth != null) _playerHealth.TakeDamage(damage);
        if (DamageFlashEffect.instance != null) DamageFlashEffect.instance.StartFlash();
        if (CameraShake.instance != null) CameraShake.instance.Shake(0.2f, 0.2f);
        if (_hurtSound != null) PlaySound(_hurtSound);
        _lastDamageTime = Time.time;
    }

    void Shoot()
    {
        if (_firePoint != null && _bulletPrefab != null)
        {
            Instantiate(_bulletPrefab, _firePoint.position, _firePoint.rotation);
            PlaySound(_shootSound);
        }
    }

    void UseHomingSkill()
    {
        if (_firePoint != null && _homingBulletPrefab != null)
        {
            Instantiate(_homingBulletPrefab, _firePoint.position, _firePoint.rotation);
            PlaySound(_homingShootSound != null ? _homingShootSound : _shootSound);
            _cooldownTimer = _skillCooldown;
        }
    }

    void HandleChargeAttack(bool isOverUI)
    {
        if (Input.GetMouseButtonDown(1) && !isOverUI)
        {
            _isCharging = true;
            _currentChargeTime = 0f;
            if (_chargeSound != null)
            {
                _chargeAudioSource.clip = _chargeSound;
                _chargeAudioSource.pitch = 0.8f;
                _chargeAudioSource.Play();
            }
        }

        if (_isCharging && Input.GetMouseButton(1))
        {
            _currentChargeTime += Time.deltaTime;
            float chargeRatio = Mathf.Clamp01(_currentChargeTime / _chargeTimeRequired);

            if (_characterVisual != null)
            {
                float currentMagnitude = _shakeMagnitude * chargeRatio;
                float offsetX = Mathf.Sin(Time.time * _shakeSpeed) * currentMagnitude;
                float offsetY = Mathf.Cos(Time.time * _shakeSpeed * 1.2f) * currentMagnitude;
                _characterVisual.localPosition = _visualInitialPos + new Vector3(offsetX, offsetY, 0);
            }

            if (_chargeAudioSource.isPlaying)
                _chargeAudioSource.pitch = 0.8f + (chargeRatio * 0.7f);
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (_isCharging && _currentChargeTime >= _chargeTimeRequired)
            {
                FireLaserAtMouse();
                if (CameraShake.instance != null) CameraShake.instance.Shake(0.2f, 0.3f);
            }
            StopChargeEffects();
        }
    }

    void StopChargeEffects()
    {
        _isCharging = false;
        _currentChargeTime = 0f;
        if (_chargeAudioSource != null && _chargeAudioSource.isPlaying) _chargeAudioSource.Stop();
        if (_characterVisual != null) _characterVisual.localPosition = _visualInitialPos;
    }

    void FireLaserAtMouse()
    {
        if (_laserPrefab != null && _firePoint != null)
        {
            Vector3 mousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector3 fireDirection = (mousePos - _firePoint.position).normalized;

            GameObject laserObj = Instantiate(_laserPrefab, _firePoint.position, Quaternion.identity);
            var beam = laserObj.GetComponent<LaserBeam>();
            if (beam != null) beam.Fire(_firePoint.position, fireDirection, _laserRange);

            PlaySound(_laserShootSound != null ? _laserShootSound : _shootSound);
        }
    }

    void HandleFootstepSound()
    {
        if (_footstepClip == null || _moveInput.magnitude < 0.1f) { _isStepReady = false; return; }
        if (!_isStepReady) { _footstepTimer = _footstepDelay; _isStepReady = true; }

        _footstepTimer -= Time.deltaTime;
        if (_footstepTimer <= 0)
        {
            PlaySound(_footstepClip);
            _footstepTimer = _footstepDelay;
        }
    }

    void HandleCooldownUI()
    {
        if (_cooldownImage == null) return;
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
            _cooldownImage.fillAmount = _cooldownTimer / _skillCooldown;
        }
        else _cooldownImage.fillAmount = 0;
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
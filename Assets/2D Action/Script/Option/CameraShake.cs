using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    // --- Singleton Instance ---
    public static CameraShake instance;

    // --- Private Variables ---
    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;
    private Coroutine currentShakeCoroutine;

    // --- Unity Life Cycle ---

    void Awake()
    {
        // ตั้งค่า Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // บันทึกตำแหน่งและการหมุนเริ่มต้นไว้สำหรับคืนค่า
        initialLocalPos = transform.localPosition;
        initialLocalRot = transform.localRotation;
    }

    // --- Public Methods ---

    public void Shake(float duration, float magnitude)
    {
        // ถ้ามีการสั่นเดิมอยู่ ให้หยุดก่อนเพื่อเริ่มใหม่
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);

            // คืนค่าตำแหน่งก่อนเริ่มใหม่ป้องกันกล้องเยื้อง
            transform.localPosition = initialLocalPos;
            transform.localRotation = initialLocalRot;
        }

        currentShakeCoroutine = StartCoroutine(ProcessShake(duration, magnitude));
    }

    // --- Private Methods (Coroutines) ---

    private IEnumerator ProcessShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        float randomSeed = Random.Range(0f, 100f);

        while (elapsed < duration)
        {
            // Damper: ทำให้การสั่นค่อยๆ เบาลงตามเวลา
            float damper = 1.0f - (elapsed / duration);

            // Perlin Noise: คำนวณการสั่นแบบนุ่มนวล (Smooth)
            float x = (Mathf.PerlinNoise(randomSeed, elapsed * 25f) * 2f - 1f) * magnitude * damper;
            float y = (Mathf.PerlinNoise(randomSeed + 1, elapsed * 25f) * 2f - 1f) * magnitude * damper;
            float zRot = (Mathf.PerlinNoise(randomSeed + 2, elapsed * 25f) * 2f - 1f) * (magnitude * 10f) * damper;

            // ประยุกต์ใช้ค่าที่คำนวณได้
            transform.localPosition = initialLocalPos + new Vector3(x, y, 0);
            transform.localRotation = initialLocalRot * Quaternion.Euler(0, 0, zRot);

            // ใช้ unscaledDeltaTime เพื่อให้ทำงานได้แม้หยุดเวลาเกม (Time.timeScale = 0)
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // คืนค่าตำแหน่งและการหมุนกลับสู่สภาวะปกติ
        transform.localPosition = initialLocalPos;
        transform.localRotation = initialLocalRot;
        currentShakeCoroutine = null;
    }
}
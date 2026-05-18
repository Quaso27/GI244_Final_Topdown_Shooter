using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;
    private Coroutine currentShakeCoroutine;


    void Awake()
    {
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
        initialLocalPos = Vector3.zero; 
        initialLocalRot = Quaternion.identity;
    }

    public void Shake(float duration, float magnitude)
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);

            transform.localPosition = initialLocalPos;
            transform.localRotation = initialLocalRot;
        }

        currentShakeCoroutine = StartCoroutine(ProcessShake(duration, magnitude));
    }

    private IEnumerator ProcessShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        float randomSeed = Random.Range(0f, 100f);

        while (elapsed < duration)
        {
            float damper = 1.0f - (elapsed / duration);

            float x = (Mathf.PerlinNoise(randomSeed, elapsed * 25f) * 2f - 1f) * magnitude * damper;
            float y = (Mathf.PerlinNoise(randomSeed + 1, elapsed * 25f) * 2f - 1f) * magnitude * damper;
            float zRot = (Mathf.PerlinNoise(randomSeed + 2, elapsed * 25f) * 2f - 1f) * (magnitude * 10f) * damper;

            transform.localPosition = initialLocalPos + new Vector3(x, y, 0);
            transform.localRotation = initialLocalRot * Quaternion.Euler(0, 0, zRot);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = initialLocalPos;
        transform.localRotation = initialLocalRot;
        currentShakeCoroutine = null;
    }
}
using UnityEngine;
using UnityEngine.UI;

public class DamageFlashEffect : MonoBehaviour
{
    public static DamageFlashEffect instance;

    [Header("UI Reference")]
    public Image flashImage;      

    [Header("Settings")]
    public float flashSpeed = 5f; 

    public Color flashColor = new Color(1f, 1f, 1f, 0.8f);

    private bool canFlash = true; 

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ResetFlash();
    }

    void Update()
    {
        if (canFlash && flashImage != null && flashImage.color.a > 0)
        {
            flashImage.color = Color.Lerp(flashImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }
    }

    public void StartFlash()
    {
        if (flashImage != null && canFlash)
        {
            flashImage.color = flashColor;
        }
    }

    public void StopAndClear()
    {
        canFlash = false; 
        ResetFlash();     
    }

    public void ResetEffectState()
    {
        canFlash = true;
        ResetFlash();
    }

    public void ResetFlash()
    {
        if (flashImage != null)
        {
            Color c = flashImage.color;
            flashImage.color = new Color(c.r, c.g, c.b, 0f);
        }
    }
}
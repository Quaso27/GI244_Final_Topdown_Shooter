using UnityEngine;
using TMPro;

public class BlinkingText : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI textToBlink; 

    [Header("Settings")]
    public float blinkSpeed = 1.5f; 

    void Update()
    {
        if (textToBlink != null)
        {
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1.0f);
            Color newColor = textToBlink.color;
            newColor.a = alpha;
            textToBlink.color = newColor;
        }
    }
}
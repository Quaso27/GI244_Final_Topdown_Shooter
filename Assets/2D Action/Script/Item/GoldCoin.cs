using UnityEngine;

public class GoldCoin : MonoBehaviour
{
    public int goldValue = 1; // หนึ่งเหรียญมีค่าเท่าไหร่

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.AddGold(goldValue);
            }
            Destroy(gameObject);
        }
    }
}
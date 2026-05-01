using UnityEngine;
using System.Collections.Generic;

public class ItemDropper : MonoBehaviour
{
    [System.Serializable]
    public class DropItem
    {
        public GameObject itemPrefab;
        [Range(0, 100)] public float chance;
    }

    public List<DropItem> lootTable = new List<DropItem>();

    // ถูกเรียกอัตโนมัติจาก EnemyBase เมื่อ Die() ทำงาน
    public void OnEnemyDeath()
    {
        foreach (var loot in lootTable)
        {
            if (Random.Range(0f, 100f) <= loot.chance)
            {
                if (loot.itemPrefab != null)
                {
                    Instantiate(loot.itemPrefab, transform.position, Quaternion.identity);
                }
            }
        }
    }
}
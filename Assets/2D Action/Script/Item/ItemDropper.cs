using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [System.Serializable]
    public class DropItem
    {
        public GameObject itemPrefab;
        public float chance;
    }

    [Header("Loot Settings")]
    [SerializeField]
    private DropItem[] _lootTable; 

    public void OnEnemyDeath()
    {
        if (_lootTable == null || _lootTable.Length == 0) return;

        foreach (var loot in _lootTable)
        {
            if (loot.itemPrefab == null || loot.chance <= 0) continue;

            if (Random.Range(0f, 100f) <= loot.chance)
            {
                SpawnItem(loot.itemPrefab);
            }
        }
    }

    private void SpawnItem(GameObject prefab)
    {
        Instantiate(prefab, transform.position, Quaternion.identity);
    }
}
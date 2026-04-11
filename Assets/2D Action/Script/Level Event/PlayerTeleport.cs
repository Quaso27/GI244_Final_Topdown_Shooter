using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerTeleport : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private Transform teleportTransform;

    // Start is called before the first frame update
    void Start()
    {
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6 && collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.position = teleportTransform.position;
        }
    }
}

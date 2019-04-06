using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Shuriken : NetworkBehaviour
{
    public float speed;
    public GameObject owner;
    public float lifetime;
    private Rigidbody2D rb;

    private void Start()
    {
        if (isServer)
        {
            rb = GetComponent<Rigidbody2D>();
            rb.velocity = transform.right * speed;
            StartCoroutine(DestroyShuriken());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isServer && collision.gameObject.CompareTag("Player") && collision.gameObject.GetInstanceID() != owner.GetInstanceID() && collision.gameObject.GetComponent<PlayerHealth>().isAlive)
        {
            collision.gameObject.GetComponent<PlayerHealth>().Kill();
            owner.GetComponent<PlayerInfo>().killCount++;
            NetworkServer.Destroy(gameObject);
        }
    }

    private IEnumerator DestroyShuriken()
    {
        yield return new WaitForSeconds(lifetime);
        NetworkServer.Destroy(gameObject);
        yield break;
    }

}

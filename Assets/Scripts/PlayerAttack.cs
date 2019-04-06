using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerAttack : NetworkBehaviour
{
    public float debounceTime;
    public GameObject shuriken;
    private bool debouncing;
    private PlayerHealth playerHealth;

    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void ShootShuriken()
    {
        if (isLocalPlayer)
        {
            CmdShootShuriken(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    [Command]
    private void CmdShootShuriken(Vector3 mousePosition)
    {
        if (!debouncing && playerHealth.isAlive)
        {
            StartCoroutine(Debounce());
            GameObject instantiatedShuriken = Instantiate(
                shuriken,
                transform.position,
                Quaternion.FromToRotation(Vector2.right, (Vector2)mousePosition - (Vector2)transform.position)
                );
            instantiatedShuriken.GetComponent<Shuriken>().owner = gameObject;
            NetworkServer.Spawn(instantiatedShuriken);
        }

    }

    private IEnumerator Debounce()
    {
        debouncing = true;
        yield return new WaitForSeconds(debounceTime);
        debouncing = false;
        yield break;

    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            ShootShuriken();
        }
    }

}

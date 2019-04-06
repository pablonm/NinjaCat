using UnityEngine;
using Mirror;

public class PlayerHealth : NetworkBehaviour
{
    [SyncVar]
    public bool isAlive = true;
    public GameObject tryAgainCanvas;
    public AudioClip dieSFX;

    private Animator anim;
    private PlayerInfo playerInfo;
    private Vector3 nextSpawn;

    private void Start()
    {
        anim = GetComponent<Animator>();
        playerInfo = GetComponent<PlayerInfo>();
        if (isServer)
        {
            SetRandomPosition();
        }
    }

    public void Kill()
    {
        isAlive = false;
        anim.SetTrigger("die");
        RpcKill();
    }

    [ClientRpc]
    private void RpcKill()
    {
        GetComponent<AudioSource>().clip = dieSFX;
        GetComponent<AudioSource>().Play();
        if (isLocalPlayer)
        {
            tryAgainCanvas.SetActive(true);
        }
    }

    public void Revive()
    {
        CmdRevive();
    }

    [Command]
    private void CmdRevive()
    {
        isAlive = true;
        anim.SetTrigger("revive");
        playerInfo.killCount = 0;
        RpcRevive();
        SetRandomPosition();
    }

    [ClientRpc]
    private void RpcRevive()
    {
        if (isLocalPlayer)
        {   
            tryAgainCanvas.SetActive(false);
        }
    }

    private void SetRandomPosition()
    {
        Vector3 newSpawn = new Vector3(Random.Range(-24f, 24f), Random.Range(-24f, 24f), transform.position.z);
        while (Mathf.Abs(transform.position.magnitude - newSpawn.magnitude) < 5f) {
            newSpawn = new Vector3(Random.Range(-24f, 24f), Random.Range(-24f, 24f), transform.position.z);
        }
        transform.position = new Vector3(Random.Range(-24f, 24f), Random.Range(-24f, 24f), transform.position.z);
    }
}

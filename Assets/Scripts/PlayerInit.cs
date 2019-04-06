using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerInit : NetworkBehaviour
{
    public GameObject killCountCanvas;
    private PlayerInfo playerInfo;

	private void Start ()
    {
        playerInfo = GetComponent<PlayerInfo>();
        InitCameraFollow();
        GetName();
        HideLogin();
        InitKillCount();
    }

    private void InitCameraFollow()
    {
        if (isLocalPlayer)
        {
            FindObjectOfType<CameraFollow>().target = transform;
        }
    }

    private void GetName()
    {
        if (isLocalPlayer)
        {
            CmdSetName(FindObjectOfType<Login>().nameInput.text);
        }
    }

    [Command]
    private void CmdSetName(string name)
    {
        playerInfo.playerName = name;
    }

    private void HideLogin()
    {
        if (isLocalPlayer)
        {
            FindObjectOfType<Login>().gameObject.SetActive(false);
        }
    }

    private void InitKillCount()
    {
        if (isLocalPlayer)
        {
            killCountCanvas.SetActive(true);
        }
    }
}

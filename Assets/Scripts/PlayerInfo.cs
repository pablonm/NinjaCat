using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerInfo : NetworkBehaviour
{
    [SyncVar]
    public string playerName;
    public Text nameText;

    [SyncVar]
    public int killCount;
    public Text killCountText;


    private void Update()
    {
        nameText.text = playerName;
        killCountText.text = killCount.ToString();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Login : MonoBehaviour
{
    public InputField nameInput;
    public NetworkManager networkManager;

    public void StartGame()
    {
        if (nameInput.text.Length > 0)
        {
            networkManager.StartClient();
        }
    }
}

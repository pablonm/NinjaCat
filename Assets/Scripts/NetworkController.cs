using UnityEngine;
using Mirror;

public class NetworkController : MonoBehaviour
{
    public enum NETWORK_ENTITY { SERVER, CLIENT };
    public NETWORK_ENTITY entity;
    public GameObject loginCanvas;

    private NetworkManager networkManager;

    private void Start ()
    {
        networkManager = GetComponent<NetworkManager>();
        if (entity == NETWORK_ENTITY.SERVER)
        {
            networkManager.StartServer();
        }
        else
        {
            loginCanvas.SetActive(true);
        }
	}

    /*private void OnApplicationQuit()
    {
        if (entity == NETWORK_ENTITY.CLIENT)
        {
            networkManager.StopClient();
        }
        else
        {
            networkManager.StopServer();
        }
    }*/

}

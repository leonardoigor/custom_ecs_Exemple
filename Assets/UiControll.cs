using UnityEngine;
using Unity.Netcode;

public class UiControll : MonoBehaviour
{
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        gameObject.AddComponent<ServerBootstrap>();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        gameObject.AddComponent<ClientBootstrap>();
    }
}

using UnityEngine;
using Unity.Netcode;

namespace MiniECS.Game.Authoring
{
    public class PlayerAuthoring : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Vector3 spawnPosition = new Vector3(0, 1, 0);

        private void Start()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

                // Se o servidor já estiver rodando (ex: Host iniciou antes deste script), spawnar para quem já está conectado
                if (NetworkManager.Singleton.IsServer)
                {
                    foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
                    {
                        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
                        {
                            if (client.PlayerObject == null)
                            {
                                SpawnPlayer(clientId);
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("[PlayerAuthoring] NetworkManager.Singleton not found!");
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                SpawnPlayer(clientId);
            }
        }

        private void SpawnPlayer(ulong clientId)
        {
            if (playerPrefab == null)
            {
                Debug.LogError("[PlayerAuthoring] Player Prefab is missing!");
                return;
            }

            Debug.Log($"[PlayerAuthoring] Spawning player for ClientId: {clientId}");
            var instance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            var netObj = instance.GetComponent<NetworkObject>();
            
            // Spawna como objeto do jogador (Player Object)
            netObj.SpawnAsPlayerObject(clientId);
        }
    }
}

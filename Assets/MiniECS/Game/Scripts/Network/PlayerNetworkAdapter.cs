using Game.Components;
using MiniECS;
using Unity.Netcode;
using UnityEngine;

namespace Game.Network
{
    public class PlayerNetworkAdapter : NetworkBehaviour
    {
        [SerializeField] private Entity entity;
        [SerializeField] private Animator animator;

        // Sync velocity from Server to Clients for animation
        private NetworkVariable<Vector2> netVelocity = new NetworkVariable<Vector2>(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            animator = GetComponent<Animator>();
            if (animator == null) return;
            entity = EntityManager.CreateEntity();


            if (IsServer)
            {
                // Server manages logic and transform
                EntityManager.Add(entity, new Position { Value = transform.position });
                EntityManager.Add(entity, new Velocity());
                EntityManager.Add(entity, new InputData());
                EntityManager.Add(entity, new TransformRef { Value = transform });
            }

            if (IsClient)
            {
                // Client only visualizes (Animation)
                // IMPORTANTE: Não adicionamos Position/TransformRef aqui no Cliente.
                // A posição é controlada pelo NetworkTransform (Server Authoritative).
                EntityManager.Add(entity, new Velocity());
                EntityManager.Add(entity, new AnimatorRef { Value = animator });
            }
        }

        private void Update()
        {
            if (entity.Id == 0) return; // Wait for Entity creation

            if (IsOwner && IsClient)
            {
                var x = Input.GetAxis("Horizontal");
                var y = Input.GetAxis("Vertical");
                var input = new Vector2(x, y);

                if (input != Vector2.zero || Time.frameCount % 5 == 0)
                {
                    SubmitInputServerRpc(input);
                }
            }

            if (IsServer)
            {
                if (EntityManager.Has<Velocity>(entity.Id))
                {
                    netVelocity.Value = EntityManager.Pool<Velocity>().Get(entity.Id).Value;
                }
            }

            if (IsClient)
            {
                if (EntityManager.Has<Velocity>(entity.Id))
                {
                    EntityManager.Pool<Velocity>().Get(entity.Id).Value = netVelocity.Value;
                }
            }
        }

        [ServerRpc]
        private void SubmitInputServerRpc(Vector2 input)
        {
            // Debug para confirmar se o input chegou no servidor
            // Debug.Log($"[Server] Received Input: {input}"); 
            
            if (EntityManager.Has<InputData>(entity.Id))
            {
                EntityManager.Pool<InputData>().Get(entity.Id).Move = input;
                // Força a velocidade imediatamente para teste (pode ser removido depois)
                // EntityManager.Pool<Velocity>().Get(entity.Id).Value = input * 5f; 
            }
        }
    }
}

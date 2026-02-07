using MiniECS;
using Game.Components;
using UnityEngine;

namespace Game.Systems
{
    // Syncs ECS Position -> Transform (Server only)
    public struct ServerPositionSyncSystem : ISystem, IServerSystem, IMainThreadSystem
    {
        public void OnCreate(ref SystemContext ctx) { }

        public void OnUpdate(ref SystemContext ctx)
        {
            foreach (var (e, pos, trans) in EntityManager.Query<Position, TransformRef>().ForEach())
            {
                trans.Value.position = pos.Value;
            }
        }

        public void OnDestroy(ref SystemContext ctx) { }
    }
}

using MiniECS;
using Game.Components;
using UnityEngine;

namespace Game.Systems
{
    public struct PositionSystemClient : ISystem, IClientSystem, IMainThreadSystem
    {
        public void OnCreate(ref SystemContext ctx) {}
        
        public void OnUpdate(ref SystemContext ctx) 
        {
            // Sync ECS Position -> Unity Transform
            foreach (var (e, pos, trans) in EntityManager.Query<Position, TransformRef>().ForEach()) 
            {
                trans.Value.position = pos.Value;
            }
        }
        
        public void OnDestroy(ref SystemContext ctx) {}
    }
}

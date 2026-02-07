using MiniECS;
using Game.Components;
using UnityEngine;

namespace Game.Systems
{
    public struct PlayerMovementSystem : ISystem, IServerSystem
    {
        public void OnCreate(ref SystemContext ctx) { }

        public void OnUpdate(ref SystemContext ctx)
        {
            // Update Velocity based on Input
            foreach (var (e, input, vel) in EntityManager.Query<InputData, ValueRW<Velocity>>().ForEach())
            {
                vel.Value.Value = input.Move * 5f; // Speed 5
            }

            // Update Position based on Velocity
            foreach (var (e, pos, vel) in EntityManager.Query<ValueRW<Position>, Velocity>().ForEach())
            {
                pos.Value.Value += vel.Value * ctx.DeltaTime;
            }
        }

        public void OnDestroy(ref SystemContext ctx) { }
    }
}

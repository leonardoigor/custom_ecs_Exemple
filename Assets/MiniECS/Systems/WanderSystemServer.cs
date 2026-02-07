
using MiniECS;
using UnityEngine;

public struct WanderSystemServer : ISystem, IServerSystem {
    public void OnCreate(ref SystemContext ctx) {}
    public void OnUpdate(ref SystemContext ctx) {
        foreach (var (e, pos) in EntityManager.Query<ValueRW<Position>>().ForEach()) {
            //pos.Value.Value += (Vector2)Vector3.right * ctx.DeltaTime;
        }
    }
    public void OnDestroy(ref SystemContext ctx) {}
}

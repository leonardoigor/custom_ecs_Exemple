using Game.Components;
using MiniECS;
using UnityEngine;

namespace Game.Systems
{
    public struct AnimationSystemClient : ISystem, IClientSystem, IMainThreadSystem {
        public void OnCreate(ref SystemContext ctx) {}
        public void OnUpdate(ref SystemContext ctx) {
            foreach (var (e, anim, vel) in EntityManager.Query<AnimatorRef, Velocity>().ForEach()) {
                if (anim.Value is null) continue;
                anim.Value.SetBool("Run", vel.Value.magnitude > 0.1f);
                Debug.Log($"{anim.Value}-{vel.Value.magnitude > 0.1f}");
            }
        }
        public void OnDestroy(ref SystemContext ctx) {}
    }
}

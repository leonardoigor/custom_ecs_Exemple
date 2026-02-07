using UnityEngine;
using MiniECS;

namespace Game.Authoring
{
    public abstract class EcsAuthoring<T> : MonoBehaviour where T : struct
    {
        protected Entity Entity;
        protected abstract T Bake();

        void Awake()
        {
            Entity = EntityManager.CreateEntity();
            EntityManager.Add(Entity, Bake());
        }
    }
}

using UnityEngine;

namespace MiniECS.Authoring
{
    public abstract class EcsAuthoring<T> : MonoBehaviour where T : struct
    {
        protected MiniECS.Entity Entity;
        protected abstract T Bake();

        void Awake()
        {
            Entity = MiniECS.EntityManager.CreateEntity();
            MiniECS.EntityManager.Add(Entity, Bake());
        }
    }
}
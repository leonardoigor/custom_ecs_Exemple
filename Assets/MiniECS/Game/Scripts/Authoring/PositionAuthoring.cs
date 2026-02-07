using UnityEngine;
using MiniECS;
using Game.Components;

namespace Game.Authoring
{
    public class PositionAuthoring : MonoBehaviour
    {
        void Awake()
        {
            var entity = EntityManager.CreateEntity();
            EntityManager.Add(entity, new Position { Value = transform.position });
            EntityManager.Add(entity, new TransformRef { Value = transform });
            EntityManager.Add(entity, new Velocity { Value =  Vector3.zero});
        }
    }
}

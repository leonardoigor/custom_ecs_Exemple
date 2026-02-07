using System;
using System.Collections.Generic;

namespace MiniECS
{
    public interface IComponentPool
    {
        Array GetComponents();
        List<int> GetEntities();
        Dictionary<int, int> GetLookup();
        bool Has(int id);
    }

    public sealed class ComponentPool<T> : IComponentPool
    {
        public readonly List<int> entities = new();
        public T[] components = new T[128];
        public Dictionary<int,int> lookup = new();

        public Array GetComponents() => components;
        public List<int> GetEntities() => entities;
        public Dictionary<int, int> GetLookup() => lookup;

        public void Add(int id, T c)
        {
            lookup[id] = entities.Count;
            entities.Add(id);
            
            if (entities.Count > components.Length)
                Array.Resize(ref components, components.Length * 2);
            
            components[entities.Count - 1] = c;
        }

        public void Remove(int id)
        {
            if (!lookup.TryGetValue(id, out int i)) return;
            int last = entities.Count - 1;

            entities[i] = entities[last];
            components[i] = components[last];
            lookup[entities[i]] = i;

            entities.RemoveAt(last);
            components[last] = default; // Clear reference to allow GC
            lookup.Remove(id);
        }

        public ref T Get(int id) => ref components[lookup[id]];
        public bool Has(int id) => lookup.ContainsKey(id);
    }
}

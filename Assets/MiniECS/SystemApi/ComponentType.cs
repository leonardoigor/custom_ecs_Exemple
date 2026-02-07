using System;
using System.Collections.Generic;

namespace MiniECS
{
    internal static class ComponentRegistry
    {
        static int nextId;
        static Dictionary<Type,int> map = new();

        public static int Register(Type t)
        {
            if (!map.TryGetValue(t, out int id))
            {
                id = nextId++;
                map[t] = id;
            }
            return id;
        }
    }

    public static class ComponentType<T>
    {
        public static readonly int Id;
        public static readonly int Word;
        public static readonly ulong Bit;

        static ComponentType()
        {
            Id = ComponentRegistry.Register(typeof(T));
            Word = Id / 64;
            Bit = 1UL << (Id % 64);
        }
    }
}
using System;

namespace MiniECS
{
    public struct ValueRW<T>
        where T : struct
    {
        T[] components;
        int index;

        public ValueRW(T[] components, int index)
        {
            this.components = components;
            this.index = index;
        }

        public ValueRW(Array components, int index)
        {
            this.components = (T[])components;
            this.index = index;
        }

        public ref T Value => ref components[index];
    }
}
using System;
using System.Collections.Generic;

namespace MiniECS
{
    // =========================
    // Query<T1>
    // =========================
    public struct Query<T1>
    {
        ulong[] none;

        public Query<T1> None<T>()
        {
            BitMask.EnsureSize(ref none, ComponentType<T>.Word);
            none[ComponentType<T>.Word] |= ComponentType<T>.Bit;
            return this;
        }

        public void ForEach<TP>(TP p)
            where TP : struct, IProcessor<T1>
        {
            var p1 = EntityManager.Pool<T1>();
            var s1 = p1.components;

            for (int i = 0; i < p1.entities.Count; i++)
            {
                int id = p1.entities[i];
                if (!BitMask.HasNone(EntityManager.Mask(id), none)) continue;

                p.Execute(new Entity(id), ref s1[i]);
            }
        }

        public Enumerator ForEach() => new Enumerator(this);

        public struct Enumerator
        {
            List<int> entities;
            ulong[] none;
            int count;
            int index;

            T1[] s1;
            Array s1Rw;
            Func<Array, int, T1> create1;
            bool isRw1;

            public Enumerator(Query<T1> q)
            {
                none = q.none;
                index = -1;

                if (QueryHelper<T1>.IsValueRW)
                {
                    isRw1 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T1>.InnerType);
                    s1Rw = pool.GetComponents();
                    entities = pool.GetEntities();
                    create1 = QueryHelper<T1>.Create;
                    s1 = null;
                }
                else
                {
                    isRw1 = false;
                    var pool = EntityManager.Pool<T1>();
                    s1 = pool.components;
                    entities = pool.entities;
                    s1Rw = null;
                    create1 = null;
                }
                count = entities.Count;
            }

            public Enumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                while (++index < count)
                {
                    int id = entities[index];
                    if (!BitMask.HasNone(EntityManager.Mask(id), none)) continue;
                    return true;
                }
                return false;
            }

            public (Entity, T1) Current
            {
                get
                {
                    int id = entities[index];
                    return (new Entity(id), isRw1 ? create1(s1Rw, index) : s1[index]);
                }
            }
        }
    }

    // =========================
    // Query<T1, T2>
    // =========================
    public struct Query<T1, T2>
    {
        ulong[] none;

        public Query<T1, T2> None<T>()
        {
            BitMask.EnsureSize(ref none, ComponentType<T>.Word);
            none[ComponentType<T>.Word] |= ComponentType<T>.Bit;
            return this;
        }

        public void ForEach<TP>(TP p)
            where TP : struct, IProcessor<T1, T2>
        {
            var p1 = EntityManager.Pool<T1>();
            var p2 = EntityManager.Pool<T2>();

            var s1 = p1.components;

            for (int i = 0; i < p1.entities.Count; i++)
            {
                int id = p1.entities[i];
                if (!BitMask.HasNone(EntityManager.Mask(id), none)) continue;
                if (!EntityManager.Has<T2>(id)) continue;

                p.Execute(new Entity(id),
                    ref s1[i],
                    ref p2.Get(id));
            }
        }

        public Enumerator ForEach() => new Enumerator(this);

        public struct Enumerator
        {
            List<int> entities;
            ulong[] none;
            int count;
            int index;

            T1[] s1;
            Array s1Rw;
            Func<Array, int, T1> create1;
            bool isRw1;

            T2[] s2;
            Array s2Rw;
            Func<Array, int, T2> create2;
            bool isRw2;
            IComponentPool p2;
            Dictionary<int, int> lookup2;

            public Enumerator(Query<T1, T2> q)
            {
                none = q.none;
                index = -1;

                if (QueryHelper<T1>.IsValueRW)
                {
                    isRw1 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T1>.InnerType);
                    s1Rw = pool.GetComponents();
                    entities = pool.GetEntities();
                    create1 = QueryHelper<T1>.Create;
                    s1 = null;
                }
                else
                {
                    isRw1 = false;
                    var pool = EntityManager.Pool<T1>();
                    s1 = pool.components;
                    entities = pool.entities;
                    s1Rw = null;
                    create1 = null;
                }
                count = entities.Count;

                if (QueryHelper<T2>.IsValueRW)
                {
                    isRw2 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T2>.InnerType);
                    p2 = pool;
                    lookup2 = pool.GetLookup();
                    s2Rw = pool.GetComponents();
                    create2 = QueryHelper<T2>.Create;
                    s2 = null;
                }
                else
                {
                    isRw2 = false;
                    var pool = EntityManager.Pool<T2>();
                    p2 = pool;
                    lookup2 = pool.lookup;
                    s2 = pool.components;
                    s2Rw = null;
                    create2 = null;
                }
            }

            public Enumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                while (++index < count)
                {
                    int id = entities[index];
                    if (!BitMask.HasNone(EntityManager.Mask(id), none)) continue;
                    if (!p2.Has(id)) continue;
                    return true;
                }
                return false;
            }

            public (Entity, T1, T2) Current
            {
                get
                {
                    int id = entities[index];
                    return (new Entity(id), 
                        isRw1 ? create1(s1Rw, index) : s1[index],
                        isRw2 ? create2(s2Rw, lookup2[id]) : s2[lookup2[id]]);
                }
            }
        }
    }

    // =========================
    // Query<T1, T2, T3>
    // =========================
    public struct Query<T1, T2, T3>
    {
        ulong[] none;

        public Query<T1, T2, T3> None<T>()
        {
            BitMask.EnsureSize(ref none, ComponentType<T>.Word);
            none[ComponentType<T>.Word] |= ComponentType<T>.Bit;
            return this;
        }

        public void ForEach<TP>(TP p)
            where TP : struct, IProcessor<T1, T2, T3>
        {
            // Legacy/Standard implementation
            var p1 = EntityManager.Pool<T1>();
            var p2 = EntityManager.Pool<T2>();
            var p3 = EntityManager.Pool<T3>();
            var s1 = p1.components;

            for (int i = 0; i < p1.entities.Count; i++)
            {
                int id = p1.entities[i];
                if (!BitMask.HasNone(EntityManager.Mask(id), none)) continue;
                if (!EntityManager.Has<T2>(id)) continue;
                if (!EntityManager.Has<T3>(id)) continue;

                p.Execute(new Entity(id),
                    ref s1[i],
                    ref p2.Get(id),
                    ref p3.Get(id));
            }
        }

        public Enumerator ForEach() => new Enumerator(this);

        public struct Enumerator
        {
            List<int> entities;
            ulong[] none;
            int count;
            int index;

            T1[] s1;
            Array s1Rw;
            Func<Array, int, T1> create1;
            bool isRw1;
            T2[] s2;
            Array s2Rw;
            Func<Array, int, T2> create2;
            bool isRw2;
            IComponentPool p2;
            Dictionary<int, int> lookup2;
            T3[] s3;
            Array s3Rw;
            Func<Array, int, T3> create3;
            bool isRw3;
            IComponentPool p3;
            Dictionary<int, int> lookup3;
            

            public Enumerator(Query<T1, T2, T3> q)
            {
                none = q.none;
                index = -1;

                // Setup T1 (primary)
                if (QueryHelper<T1>.IsValueRW) {
                    isRw1 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T1>.InnerType);
                    s1Rw = pool.GetComponents();
                    entities = pool.GetEntities();
                    create1 = QueryHelper<T1>.Create;
                    s1 = null;
                } else {
                    isRw1 = false;
                    var pool = EntityManager.Pool<T1>();
                    s1 = pool.components;
                    entities = pool.entities;
                    s1Rw = null;
                    create1 = null;
                }
                count = entities.Count;

                
                if (QueryHelper<T2>.IsValueRW) {
                    isRw2 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T2>.InnerType);
                    p2 = pool;
                    lookup2 = pool.GetLookup();
                    s2Rw = pool.GetComponents();
                    create2 = QueryHelper<T2>.Create;
                    s2 = null;
                } else {
                    isRw2 = false;
                    var pool = EntityManager.Pool<T2>();
                    p2 = pool;
                    lookup2 = pool.lookup;
                    s2 = pool.components;
                    s2Rw = null;
                    create2 = null;
                }
        
                if (QueryHelper<T3>.IsValueRW) {
                    isRw3 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T3>.InnerType);
                    p3 = pool;
                    lookup3 = pool.GetLookup();
                    s3Rw = pool.GetComponents();
                    create3 = QueryHelper<T3>.Create;
                    s3 = null;
                } else {
                    isRw3 = false;
                    var pool = EntityManager.Pool<T3>();
                    p3 = pool;
                    lookup3 = pool.lookup;
                    s3 = pool.components;
                    s3Rw = null;
                    create3 = null;
                }
        
            }

            public Enumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                while (++index < count)
                {
                    int id = entities[index];
                    if (!BitMask.HasNone(EntityManager.Mask(id), none)) continue;
                    if (!p2.Has(id)) continue;
                    if (!p3.Has(id)) continue;
                    return true;
                }
                return false;
            }

            public (Entity, T1, T2, T3) Current
            {
                get
                {
                    int id = entities[index];
                    return (new Entity(id), 
                        isRw1 ? create1(s1Rw, index) : s1[index],
                        isRw2 ? create2(s2Rw, lookup2[id]) : s2[lookup2[id]],
                        isRw3 ? create3(s3Rw, lookup3[id]) : s3[lookup3[id]]);
                }
            }
        }
    }

    // =========================
    // Query<T1, T2, T3, T4>
    // =========================
    public struct Query<T1, T2, T3, T4>
    {
        ulong[] none;

        public Query<T1, T2, T3, T4> None<T>()
        {
            BitMask.EnsureSize(ref none, ComponentType<T>.Word);
            none[ComponentType<T>.Word] |= ComponentType<T>.Bit;
            return this;
        }

        public void ForEach<TP>(TP p)
            where TP : struct, IProcessor<T1, T2, T3, T4>
        {
            // Legacy/Standard implementation
            var p1 = EntityManager.Pool<T1>();
            var p2 = EntityManager.Pool<T2>();
            var p3 = EntityManager.Pool<T3>();
            var p4 = EntityManager.Pool<T4>();
            var s1 = p1.components;

            for (int i = 0; i < p1.entities.Count; i++)
            {
                int id = p1.entities[i];
                if (!BitMask.HasNone(EntityManager.Mask(id), none)) continue;
                if (!EntityManager.Has<T2>(id)) continue;
                if (!EntityManager.Has<T3>(id)) continue;
                if (!EntityManager.Has<T4>(id)) continue;

                p.Execute(new Entity(id),
                    ref s1[i],
                    ref p2.Get(id),
                    ref p3.Get(id),
                    ref p4.Get(id));
            }
        }

        public Enumerator ForEach() => new Enumerator(this);

        public struct Enumerator
        {
            List<int> entities;
            ulong[] none;
            int count;
            int index;

            T1[] s1;
            Array s1Rw;
            Func<Array, int, T1> create1;
            bool isRw1;
            T2[] s2;
            Array s2Rw;
            Func<Array, int, T2> create2;
            bool isRw2;
            IComponentPool p2;
            Dictionary<int, int> lookup2;
            T3[] s3;
            Array s3Rw;
            Func<Array, int, T3> create3;
            bool isRw3;
            IComponentPool p3;
            Dictionary<int, int> lookup3;
            T4[] s4;
            Array s4Rw;
            Func<Array, int, T4> create4;
            bool isRw4;
            IComponentPool p4;
            Dictionary<int, int> lookup4;
            

            public Enumerator(Query<T1, T2, T3, T4> q)
            {
                none = q.none;
                index = -1;

                // Setup T1 (primary)
                if (QueryHelper<T1>.IsValueRW) {
                    isRw1 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T1>.InnerType);
                    s1Rw = pool.GetComponents();
                    entities = pool.GetEntities();
                    create1 = QueryHelper<T1>.Create;
                    s1 = null;
                } else {
                    isRw1 = false;
                    var pool = EntityManager.Pool<T1>();
                    s1 = pool.components;
                    entities = pool.entities;
                    s1Rw = null;
                    create1 = null;
                }
                count = entities.Count;

                
                if (QueryHelper<T2>.IsValueRW) {
                    isRw2 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T2>.InnerType);
                    p2 = pool;
                    lookup2 = pool.GetLookup();
                    s2Rw = pool.GetComponents();
                    create2 = QueryHelper<T2>.Create;
                    s2 = null;
                } else {
                    isRw2 = false;
                    var pool = EntityManager.Pool<T2>();
                    p2 = pool;
                    lookup2 = pool.lookup;
                    s2 = pool.components;
                    s2Rw = null;
                    create2 = null;
                }
        
                if (QueryHelper<T3>.IsValueRW) {
                    isRw3 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T3>.InnerType);
                    p3 = pool;
                    lookup3 = pool.GetLookup();
                    s3Rw = pool.GetComponents();
                    create3 = QueryHelper<T3>.Create;
                    s3 = null;
                } else {
                    isRw3 = false;
                    var pool = EntityManager.Pool<T3>();
                    p3 = pool;
                    lookup3 = pool.lookup;
                    s3 = pool.components;
                    s3Rw = null;
                    create3 = null;
                }
        
                if (QueryHelper<T4>.IsValueRW) {
                    isRw4 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T4>.InnerType);
                    p4 = pool;
                    lookup4 = pool.GetLookup();
                    s4Rw = pool.GetComponents();
                    create4 = QueryHelper<T4>.Create;
                    s4 = null;
                } else {
                    isRw4 = false;
                    var pool = EntityManager.Pool<T4>();
                    p4 = pool;
                    lookup4 = pool.lookup;
                    s4 = pool.components;
                    s4Rw = null;
                    create4 = null;
                }
        
            }

            public Enumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                while (++index < count)
                {
                    int id = entities[index];
                    if (!BitMask.HasNone(EntityManager.Mask(id), none)) continue;
                    if (!p2.Has(id)) continue;
                    if (!p3.Has(id)) continue;
                    if (!p4.Has(id)) continue;
                    return true;
                }
                return false;
            }

            public (Entity, T1, T2, T3, T4) Current
            {
                get
                {
                    int id = entities[index];
                    return (new Entity(id), 
                        isRw1 ? create1(s1Rw, index) : s1[index],
                        isRw2 ? create2(s2Rw, lookup2[id]) : s2[lookup2[id]],
                        isRw3 ? create3(s3Rw, lookup3[id]) : s3[lookup3[id]],
                        isRw4 ? create4(s4Rw, lookup4[id]) : s4[lookup4[id]]);
                }
            }
        }
    }

    // =========================
    // Query<T1, T2, T3, T4, T5>
    // =========================
    public struct Query<T1, T2, T3, T4, T5>
    {
        ulong[] none;

        public Query<T1, T2, T3, T4, T5> None<T>()
        {
            BitMask.EnsureSize(ref none, ComponentType<T>.Word);
            none[ComponentType<T>.Word] |= ComponentType<T>.Bit;
            return this;
        }

        public void ForEach<TP>(TP p)
            where TP : struct, IProcessor<T1, T2, T3, T4, T5>
        {
            // Legacy/Standard implementation
            var p1 = EntityManager.Pool<T1>();
            var p2 = EntityManager.Pool<T2>();
            var p3 = EntityManager.Pool<T3>();
            var p4 = EntityManager.Pool<T4>();
            var p5 = EntityManager.Pool<T5>();
            var s1 = p1.components;

            for (int i = 0; i < p1.entities.Count; i++)
            {
                int id = p1.entities[i];
                if (!BitMask.HasNone(EntityManager.Mask(id), none)) continue;
                if (!EntityManager.Has<T2>(id)) continue;
                if (!EntityManager.Has<T3>(id)) continue;
                if (!EntityManager.Has<T4>(id)) continue;
                if (!EntityManager.Has<T5>(id)) continue;

                p.Execute(new Entity(id),
                    ref s1[i],
                    ref p2.Get(id),
                    ref p3.Get(id),
                    ref p4.Get(id),
                    ref p5.Get(id));
            }
        }

        public Enumerator ForEach() => new Enumerator(this);

        public struct Enumerator
        {
            List<int> entities;
            ulong[] none;
            int count;
            int index;

            T1[] s1;
            Array s1Rw;
            Func<Array, int, T1> create1;
            bool isRw1;
            T2[] s2;
            Array s2Rw;
            Func<Array, int, T2> create2;
            bool isRw2;
            IComponentPool p2;
            Dictionary<int, int> lookup2;
            T3[] s3;
            Array s3Rw;
            Func<Array, int, T3> create3;
            bool isRw3;
            IComponentPool p3;
            Dictionary<int, int> lookup3;
            T4[] s4;
            Array s4Rw;
            Func<Array, int, T4> create4;
            bool isRw4;
            IComponentPool p4;
            Dictionary<int, int> lookup4;
            T5[] s5;
            Array s5Rw;
            Func<Array, int, T5> create5;
            bool isRw5;
            IComponentPool p5;
            Dictionary<int, int> lookup5;
            

            public Enumerator(Query<T1, T2, T3, T4, T5> q)
            {
                none = q.none;
                index = -1;

                // Setup T1 (primary)
                if (QueryHelper<T1>.IsValueRW) {
                    isRw1 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T1>.InnerType);
                    s1Rw = pool.GetComponents();
                    entities = pool.GetEntities();
                    create1 = QueryHelper<T1>.Create;
                    s1 = null;
                } else {
                    isRw1 = false;
                    var pool = EntityManager.Pool<T1>();
                    s1 = pool.components;
                    entities = pool.entities;
                    s1Rw = null;
                    create1 = null;
                }
                count = entities.Count;

                
                if (QueryHelper<T2>.IsValueRW) {
                    isRw2 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T2>.InnerType);
                    p2 = pool;
                    lookup2 = pool.GetLookup();
                    s2Rw = pool.GetComponents();
                    create2 = QueryHelper<T2>.Create;
                    s2 = null;
                } else {
                    isRw2 = false;
                    var pool = EntityManager.Pool<T2>();
                    p2 = pool;
                    lookup2 = pool.lookup;
                    s2 = pool.components;
                    s2Rw = null;
                    create2 = null;
                }
        
                if (QueryHelper<T3>.IsValueRW) {
                    isRw3 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T3>.InnerType);
                    p3 = pool;
                    lookup3 = pool.GetLookup();
                    s3Rw = pool.GetComponents();
                    create3 = QueryHelper<T3>.Create;
                    s3 = null;
                } else {
                    isRw3 = false;
                    var pool = EntityManager.Pool<T3>();
                    p3 = pool;
                    lookup3 = pool.lookup;
                    s3 = pool.components;
                    s3Rw = null;
                    create3 = null;
                }
        
                if (QueryHelper<T4>.IsValueRW) {
                    isRw4 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T4>.InnerType);
                    p4 = pool;
                    lookup4 = pool.GetLookup();
                    s4Rw = pool.GetComponents();
                    create4 = QueryHelper<T4>.Create;
                    s4 = null;
                } else {
                    isRw4 = false;
                    var pool = EntityManager.Pool<T4>();
                    p4 = pool;
                    lookup4 = pool.lookup;
                    s4 = pool.components;
                    s4Rw = null;
                    create4 = null;
                }
        
                if (QueryHelper<T5>.IsValueRW) {
                    isRw5 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T5>.InnerType);
                    p5 = pool;
                    lookup5 = pool.GetLookup();
                    s5Rw = pool.GetComponents();
                    create5 = QueryHelper<T5>.Create;
                    s5 = null;
                } else {
                    isRw5 = false;
                    var pool = EntityManager.Pool<T5>();
                    p5 = pool;
                    lookup5 = pool.lookup;
                    s5 = pool.components;
                    s5Rw = null;
                    create5 = null;
                }
        
            }

            public Enumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                while (++index < count)
                {
                    int id = entities[index];
                    if (!BitMask.HasNone(EntityManager.Mask(id), none)) continue;
                    if (!p2.Has(id)) continue;
                    if (!p3.Has(id)) continue;
                    if (!p4.Has(id)) continue;
                    if (!p5.Has(id)) continue;
                    return true;
                }
                return false;
            }

            public (Entity, T1, T2, T3, T4, T5) Current
            {
                get
                {
                    int id = entities[index];
                    return (new Entity(id), 
                        isRw1 ? create1(s1Rw, index) : s1[index],
                        isRw2 ? create2(s2Rw, lookup2[id]) : s2[lookup2[id]],
                        isRw3 ? create3(s3Rw, lookup3[id]) : s3[lookup3[id]],
                        isRw4 ? create4(s4Rw, lookup4[id]) : s4[lookup4[id]],
                        isRw5 ? create5(s5Rw, lookup5[id]) : s5[lookup5[id]]);
                }
            }
        }
    }

    // =========================
    // Query<T1, T2, T3, T4, T5, T6>
    // =========================
    public struct Query<T1, T2, T3, T4, T5, T6>
    {
        ulong[] none;

        public Query<T1, T2, T3, T4, T5, T6> None<T>()
        {
            BitMask.EnsureSize(ref none, ComponentType<T>.Word);
            none[ComponentType<T>.Word] |= ComponentType<T>.Bit;
            return this;
        }

        public void ForEach<TP>(TP p)
            where TP : struct, IProcessor<T1, T2, T3, T4, T5, T6>
        {
            // Legacy/Standard implementation
            var p1 = EntityManager.Pool<T1>();
            var p2 = EntityManager.Pool<T2>();
            var p3 = EntityManager.Pool<T3>();
            var p4 = EntityManager.Pool<T4>();
            var p5 = EntityManager.Pool<T5>();
            var p6 = EntityManager.Pool<T6>();
            var s1 = p1.components;

            for (int i = 0; i < p1.entities.Count; i++)
            {
                int id = p1.entities[i];
                if (!BitMask.HasNone(EntityManager.Mask(id), none)) continue;
                if (!EntityManager.Has<T2>(id)) continue;
                if (!EntityManager.Has<T3>(id)) continue;
                if (!EntityManager.Has<T4>(id)) continue;
                if (!EntityManager.Has<T5>(id)) continue;
                if (!EntityManager.Has<T6>(id)) continue;

                p.Execute(new Entity(id),
                    ref s1[i],
                    ref p2.Get(id),
                    ref p3.Get(id),
                    ref p4.Get(id),
                    ref p5.Get(id),
                    ref p6.Get(id));
            }
        }

        public Enumerator ForEach() => new Enumerator(this);

        public struct Enumerator
        {
            List<int> entities;
            ulong[] none;
            int count;
            int index;

            T1[] s1;
            Array s1Rw;
            Func<Array, int, T1> create1;
            bool isRw1;
            T2[] s2;
            Array s2Rw;
            Func<Array, int, T2> create2;
            bool isRw2;
            IComponentPool p2;
            Dictionary<int, int> lookup2;
            T3[] s3;
            Array s3Rw;
            Func<Array, int, T3> create3;
            bool isRw3;
            IComponentPool p3;
            Dictionary<int, int> lookup3;
            T4[] s4;
            Array s4Rw;
            Func<Array, int, T4> create4;
            bool isRw4;
            IComponentPool p4;
            Dictionary<int, int> lookup4;
            T5[] s5;
            Array s5Rw;
            Func<Array, int, T5> create5;
            bool isRw5;
            IComponentPool p5;
            Dictionary<int, int> lookup5;
            T6[] s6;
            Array s6Rw;
            Func<Array, int, T6> create6;
            bool isRw6;
            IComponentPool p6;
            Dictionary<int, int> lookup6;
            

            public Enumerator(Query<T1, T2, T3, T4, T5, T6> q)
            {
                none = q.none;
                index = -1;

                // Setup T1 (primary)
                if (QueryHelper<T1>.IsValueRW) {
                    isRw1 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T1>.InnerType);
                    s1Rw = pool.GetComponents();
                    entities = pool.GetEntities();
                    create1 = QueryHelper<T1>.Create;
                    s1 = null;
                } else {
                    isRw1 = false;
                    var pool = EntityManager.Pool<T1>();
                    s1 = pool.components;
                    entities = pool.entities;
                    s1Rw = null;
                    create1 = null;
                }
                count = entities.Count;

                
                if (QueryHelper<T2>.IsValueRW) {
                    isRw2 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T2>.InnerType);
                    p2 = pool;
                    lookup2 = pool.GetLookup();
                    s2Rw = pool.GetComponents();
                    create2 = QueryHelper<T2>.Create;
                    s2 = null;
                } else {
                    isRw2 = false;
                    var pool = EntityManager.Pool<T2>();
                    p2 = pool;
                    lookup2 = pool.lookup;
                    s2 = pool.components;
                    s2Rw = null;
                    create2 = null;
                }
        
                if (QueryHelper<T3>.IsValueRW) {
                    isRw3 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T3>.InnerType);
                    p3 = pool;
                    lookup3 = pool.GetLookup();
                    s3Rw = pool.GetComponents();
                    create3 = QueryHelper<T3>.Create;
                    s3 = null;
                } else {
                    isRw3 = false;
                    var pool = EntityManager.Pool<T3>();
                    p3 = pool;
                    lookup3 = pool.lookup;
                    s3 = pool.components;
                    s3Rw = null;
                    create3 = null;
                }
        
                if (QueryHelper<T4>.IsValueRW) {
                    isRw4 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T4>.InnerType);
                    p4 = pool;
                    lookup4 = pool.GetLookup();
                    s4Rw = pool.GetComponents();
                    create4 = QueryHelper<T4>.Create;
                    s4 = null;
                } else {
                    isRw4 = false;
                    var pool = EntityManager.Pool<T4>();
                    p4 = pool;
                    lookup4 = pool.lookup;
                    s4 = pool.components;
                    s4Rw = null;
                    create4 = null;
                }
        
                if (QueryHelper<T5>.IsValueRW) {
                    isRw5 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T5>.InnerType);
                    p5 = pool;
                    lookup5 = pool.GetLookup();
                    s5Rw = pool.GetComponents();
                    create5 = QueryHelper<T5>.Create;
                    s5 = null;
                } else {
                    isRw5 = false;
                    var pool = EntityManager.Pool<T5>();
                    p5 = pool;
                    lookup5 = pool.lookup;
                    s5 = pool.components;
                    s5Rw = null;
                    create5 = null;
                }
        
                if (QueryHelper<T6>.IsValueRW) {
                    isRw6 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T6>.InnerType);
                    p6 = pool;
                    lookup6 = pool.GetLookup();
                    s6Rw = pool.GetComponents();
                    create6 = QueryHelper<T6>.Create;
                    s6 = null;
                } else {
                    isRw6 = false;
                    var pool = EntityManager.Pool<T6>();
                    p6 = pool;
                    lookup6 = pool.lookup;
                    s6 = pool.components;
                    s6Rw = null;
                    create6 = null;
                }
        
            }

            public Enumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                while (++index < count)
                {
                    int id = entities[index];
                    if (!BitMask.HasNone(EntityManager.Mask(id), none)) continue;
                    if (!p2.Has(id)) continue;
                    if (!p3.Has(id)) continue;
                    if (!p4.Has(id)) continue;
                    if (!p5.Has(id)) continue;
                    if (!p6.Has(id)) continue;
                    return true;
                }
                return false;
            }

            public (Entity, T1, T2, T3, T4, T5, T6) Current
            {
                get
                {
                    int id = entities[index];
                    return (new Entity(id), 
                        isRw1 ? create1(s1Rw, index) : s1[index],
                        isRw2 ? create2(s2Rw, lookup2[id]) : s2[lookup2[id]],
                        isRw3 ? create3(s3Rw, lookup3[id]) : s3[lookup3[id]],
                        isRw4 ? create4(s4Rw, lookup4[id]) : s4[lookup4[id]],
                        isRw5 ? create5(s5Rw, lookup5[id]) : s5[lookup5[id]],
                        isRw6 ? create6(s6Rw, lookup6[id]) : s6[lookup6[id]]);
                }
            }
        }
    }

    // =========================
    // Query<T1, T2, T3, T4, T5, T6, T7>
    // =========================
    public struct Query<T1, T2, T3, T4, T5, T6, T7>
    {
        ulong[] none;

        public Query<T1, T2, T3, T4, T5, T6, T7> None<T>()
        {
            BitMask.EnsureSize(ref none, ComponentType<T>.Word);
            none[ComponentType<T>.Word] |= ComponentType<T>.Bit;
            return this;
        }

        public void ForEach<TP>(TP p)
            where TP : struct, IProcessor<T1, T2, T3, T4, T5, T6, T7>
        {
            // Legacy/Standard implementation
            var p1 = EntityManager.Pool<T1>();
            var p2 = EntityManager.Pool<T2>();
            var p3 = EntityManager.Pool<T3>();
            var p4 = EntityManager.Pool<T4>();
            var p5 = EntityManager.Pool<T5>();
            var p6 = EntityManager.Pool<T6>();
            var p7 = EntityManager.Pool<T7>();
            var s1 = p1.components;

            for (int i = 0; i < p1.entities.Count; i++)
            {
                int id = p1.entities[i];
                if (!BitMask.HasNone(EntityManager.Mask(id), none)) continue;
                if (!EntityManager.Has<T2>(id)) continue;
                if (!EntityManager.Has<T3>(id)) continue;
                if (!EntityManager.Has<T4>(id)) continue;
                if (!EntityManager.Has<T5>(id)) continue;
                if (!EntityManager.Has<T6>(id)) continue;
                if (!EntityManager.Has<T7>(id)) continue;

                p.Execute(new Entity(id),
                    ref s1[i],
                    ref p2.Get(id),
                    ref p3.Get(id),
                    ref p4.Get(id),
                    ref p5.Get(id),
                    ref p6.Get(id),
                    ref p7.Get(id));
            }
        }

        public Enumerator ForEach() => new Enumerator(this);

        public struct Enumerator
        {
            List<int> entities;
            ulong[] none;
            int count;
            int index;

            T1[] s1;
            Array s1Rw;
            Func<Array, int, T1> create1;
            bool isRw1;
            T2[] s2;
            Array s2Rw;
            Func<Array, int, T2> create2;
            bool isRw2;
            IComponentPool p2;
            Dictionary<int, int> lookup2;
            T3[] s3;
            Array s3Rw;
            Func<Array, int, T3> create3;
            bool isRw3;
            IComponentPool p3;
            Dictionary<int, int> lookup3;
            T4[] s4;
            Array s4Rw;
            Func<Array, int, T4> create4;
            bool isRw4;
            IComponentPool p4;
            Dictionary<int, int> lookup4;
            T5[] s5;
            Array s5Rw;
            Func<Array, int, T5> create5;
            bool isRw5;
            IComponentPool p5;
            Dictionary<int, int> lookup5;
            T6[] s6;
            Array s6Rw;
            Func<Array, int, T6> create6;
            bool isRw6;
            IComponentPool p6;
            Dictionary<int, int> lookup6;
            T7[] s7;
            Array s7Rw;
            Func<Array, int, T7> create7;
            bool isRw7;
            IComponentPool p7;
            Dictionary<int, int> lookup7;
            

            public Enumerator(Query<T1, T2, T3, T4, T5, T6, T7> q)
            {
                none = q.none;
                index = -1;

                // Setup T1 (primary)
                if (QueryHelper<T1>.IsValueRW) {
                    isRw1 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T1>.InnerType);
                    s1Rw = pool.GetComponents();
                    entities = pool.GetEntities();
                    create1 = QueryHelper<T1>.Create;
                    s1 = null;
                } else {
                    isRw1 = false;
                    var pool = EntityManager.Pool<T1>();
                    s1 = pool.components;
                    entities = pool.entities;
                    s1Rw = null;
                    create1 = null;
                }
                count = entities.Count;

                
                if (QueryHelper<T2>.IsValueRW) {
                    isRw2 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T2>.InnerType);
                    p2 = pool;
                    lookup2 = pool.GetLookup();
                    s2Rw = pool.GetComponents();
                    create2 = QueryHelper<T2>.Create;
                    s2 = null;
                } else {
                    isRw2 = false;
                    var pool = EntityManager.Pool<T2>();
                    p2 = pool;
                    lookup2 = pool.lookup;
                    s2 = pool.components;
                    s2Rw = null;
                    create2 = null;
                }
        
                if (QueryHelper<T3>.IsValueRW) {
                    isRw3 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T3>.InnerType);
                    p3 = pool;
                    lookup3 = pool.GetLookup();
                    s3Rw = pool.GetComponents();
                    create3 = QueryHelper<T3>.Create;
                    s3 = null;
                } else {
                    isRw3 = false;
                    var pool = EntityManager.Pool<T3>();
                    p3 = pool;
                    lookup3 = pool.lookup;
                    s3 = pool.components;
                    s3Rw = null;
                    create3 = null;
                }
        
                if (QueryHelper<T4>.IsValueRW) {
                    isRw4 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T4>.InnerType);
                    p4 = pool;
                    lookup4 = pool.GetLookup();
                    s4Rw = pool.GetComponents();
                    create4 = QueryHelper<T4>.Create;
                    s4 = null;
                } else {
                    isRw4 = false;
                    var pool = EntityManager.Pool<T4>();
                    p4 = pool;
                    lookup4 = pool.lookup;
                    s4 = pool.components;
                    s4Rw = null;
                    create4 = null;
                }
        
                if (QueryHelper<T5>.IsValueRW) {
                    isRw5 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T5>.InnerType);
                    p5 = pool;
                    lookup5 = pool.GetLookup();
                    s5Rw = pool.GetComponents();
                    create5 = QueryHelper<T5>.Create;
                    s5 = null;
                } else {
                    isRw5 = false;
                    var pool = EntityManager.Pool<T5>();
                    p5 = pool;
                    lookup5 = pool.lookup;
                    s5 = pool.components;
                    s5Rw = null;
                    create5 = null;
                }
        
                if (QueryHelper<T6>.IsValueRW) {
                    isRw6 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T6>.InnerType);
                    p6 = pool;
                    lookup6 = pool.GetLookup();
                    s6Rw = pool.GetComponents();
                    create6 = QueryHelper<T6>.Create;
                    s6 = null;
                } else {
                    isRw6 = false;
                    var pool = EntityManager.Pool<T6>();
                    p6 = pool;
                    lookup6 = pool.lookup;
                    s6 = pool.components;
                    s6Rw = null;
                    create6 = null;
                }
        
                if (QueryHelper<T7>.IsValueRW) {
                    isRw7 = true;
                    var pool = EntityManager.GetPool(QueryHelper<T7>.InnerType);
                    p7 = pool;
                    lookup7 = pool.GetLookup();
                    s7Rw = pool.GetComponents();
                    create7 = QueryHelper<T7>.Create;
                    s7 = null;
                } else {
                    isRw7 = false;
                    var pool = EntityManager.Pool<T7>();
                    p7 = pool;
                    lookup7 = pool.lookup;
                    s7 = pool.components;
                    s7Rw = null;
                    create7 = null;
                }
        
            }

            public Enumerator GetEnumerator() => this;

            public bool MoveNext()
            {
                while (++index < count)
                {
                    int id = entities[index];
                    if (!BitMask.HasNone(EntityManager.Mask(id), none)) continue;
                    if (!p2.Has(id)) continue;
                    if (!p3.Has(id)) continue;
                    if (!p4.Has(id)) continue;
                    if (!p5.Has(id)) continue;
                    if (!p6.Has(id)) continue;
                    if (!p7.Has(id)) continue;
                    return true;
                }
                return false;
            }

            public (Entity, T1, T2, T3, T4, T5, T6, T7) Current
            {
                get
                {
                    int id = entities[index];
                    return (new Entity(id), 
                        isRw1 ? create1(s1Rw, index) : s1[index],
                        isRw2 ? create2(s2Rw, lookup2[id]) : s2[lookup2[id]],
                        isRw3 ? create3(s3Rw, lookup3[id]) : s3[lookup3[id]],
                        isRw4 ? create4(s4Rw, lookup4[id]) : s4[lookup4[id]],
                        isRw5 ? create5(s5Rw, lookup5[id]) : s5[lookup5[id]],
                        isRw6 ? create6(s6Rw, lookup6[id]) : s6[lookup6[id]],
                        isRw7 ? create7(s7Rw, lookup7[id]) : s7[lookup7[id]]);
                }
            }
        }
    }

}
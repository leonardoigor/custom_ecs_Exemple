
using Unity.Jobs;
using Unity.Burst;

public sealed class SystemWrapper<T> : SystemWrapper
    where T : struct, ISystem {

    T system;

    public override bool IsServer => typeof(IServerSystem).IsAssignableFrom(typeof(T));
    public override bool IsClient => typeof(IClientSystem).IsAssignableFrom(typeof(T));

    public override void Create(ref SystemContext ctx) {
        system.OnCreate(ref ctx);
    }

    public override JobHandle Update(ref SystemContext ctx, JobHandle dep) {
        if (typeof(IMainThreadSystem).IsAssignableFrom(typeof(T)))
        {
            dep.Complete();
            system.OnUpdate(ref ctx);
            return default;
        }

        var job = new Job { System = system, Context = ctx };
        return job.Schedule(dep);
    }

    public override void Destroy(ref SystemContext ctx) {
        system.OnDestroy(ref ctx);
    }

    [BurstCompile]
    struct Job : IJob {
        public T System;
        public SystemContext Context;
        public void Execute() {
            System.OnUpdate(ref Context);
        }
    }
}

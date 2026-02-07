
using Unity.Jobs;
using Unity.Burst;

public abstract class SystemWrapper {
    public abstract bool IsServer { get; }
    public abstract bool IsClient { get; }
    public abstract void Create(ref SystemContext ctx);
    public abstract JobHandle Update(ref SystemContext ctx, JobHandle dep);
    public abstract void Destroy(ref SystemContext ctx);
}

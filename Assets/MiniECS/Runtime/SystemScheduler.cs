
using Unity.Jobs;
using System.Collections.Generic;

public sealed class SystemScheduler {
    List<SystemWrapper> active = new();
    SystemContext ctx;

    public SystemScheduler(bool server, bool client) {
        ctx = new SystemContext { IsServer = server, IsClient = client };
        foreach (var s in SystemRegistry.Systems) {
            if (server && s.IsServer) active.Add(s);
            if (client && s.IsClient) active.Add(s);
        }
        foreach (var s in active) s.Create(ref ctx);
    }

    public void Update(float dt) {
        ctx.DeltaTime = dt;
        JobHandle h = default;
        foreach (var s in active)
            h = s.Update(ref ctx, h);
        h.Complete();
    }
}

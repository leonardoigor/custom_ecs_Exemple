
public interface ISystem {
    void OnCreate(ref SystemContext ctx);
    void OnUpdate(ref SystemContext ctx);
    void OnDestroy(ref SystemContext ctx);
}

public interface IMainThreadSystem { }

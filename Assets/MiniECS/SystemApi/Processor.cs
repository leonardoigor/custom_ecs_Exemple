namespace MiniECS
{
    // =========================
    // 1 componente
    // =========================
    public interface IProcessor<T1>
    {
        void Execute(Entity e, ref T1 c1);
    }

    // =========================
    // 2 componentes
    // =========================
    public interface IProcessor<T1, T2>
    {
        void Execute(Entity e, ref T1 c1, ref T2 c2);
    }

    // =========================
    // 3 componentes
    // =========================
    public interface IProcessor<T1, T2, T3>
    {
        void Execute(Entity e, ref T1 c1, ref T2 c2, ref T3 c3);
    }

    // =========================
    // 4 componentes
    // =========================
    public interface IProcessor<T1, T2, T3, T4>
    {
        void Execute(Entity e,
            ref T1 c1,
            ref T2 c2,
            ref T3 c3,
            ref T4 c4);
    }

    // =========================
    // 5 componentes
    // =========================
    public interface IProcessor<T1, T2, T3, T4, T5>
    {
        void Execute(Entity e,
            ref T1 c1,
            ref T2 c2,
            ref T3 c3,
            ref T4 c4,
            ref T5 c5);
    }

    // =========================
    // 6 componentes
    // =========================
    public interface IProcessor<T1, T2, T3, T4, T5, T6>
    {
        void Execute(Entity e,
            ref T1 c1,
            ref T2 c2,
            ref T3 c3,
            ref T4 c4,
            ref T5 c5,
            ref T6 c6);
    }

    // =========================
    // 7 componentes
    // =========================
    public interface IProcessor<T1, T2, T3, T4, T5, T6, T7>
    {
        void Execute(Entity e,
            ref T1 c1,
            ref T2 c2,
            ref T3 c3,
            ref T4 c4,
            ref T5 c5,
            ref T6 c6,
            ref T7 c7);
    }
}

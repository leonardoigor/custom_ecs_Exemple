namespace MiniECS
{
    public readonly struct Entity
    {
        public readonly int Id;
        public Entity(int id) => Id = id;
        public override string ToString() => $"Entity({Id})";
    }
}
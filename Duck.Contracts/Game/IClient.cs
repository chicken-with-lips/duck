namespace Duck.Game
{
    public interface IClient
    {
        public void Initialize(IClientInitializationContext context);

        public void Tick();
    }
}

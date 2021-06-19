namespace Duck.Contracts.SceneManagement
{
    public interface ISceneSubsystem : IApplicationSubsystem
    {
        public IScene Create();
    }
}

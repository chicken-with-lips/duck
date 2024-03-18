namespace Duck.Platform;

public interface IPlatformFactory
{
    public IPlatform Create(IApplication application);
}

namespace Duck.Contracts.AssetManagement
{

    public interface IMaterial
    {
        public IMaterialAsset Asset { get; }
    }

    public interface IMaterialAsset : IAsset
    {
    }
}

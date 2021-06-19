using System;
using Duck.Contracts.AssetManagement;

namespace Duck.AssetManagement
{
    public struct MaterialAsset : IMaterialAsset
    {
        #region Properties

        public Guid Id { get; }
        public AssetStatus Status { get; }
        public Uri Uri { get; }

        #endregion

        #region Methods

        public MaterialAsset(Uri uri)
        {
            Id = Guid.NewGuid();
            Uri = uri;
            Status = AssetStatus.Unloaded;
        }

        #endregion
    }

    public struct Material : IMaterial
    {
        public IMaterialAsset Asset { get; }
        public Filament.Material InternalMaterial { get; }

        public Material(IMaterialAsset asset, Filament.Material internalMaterial)
        {
            Asset = asset;
            InternalMaterial = internalMaterial;
        }
    }
}

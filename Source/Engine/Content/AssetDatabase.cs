using System;
using System.Collections.Concurrent;
using Duck.AssetManagement;

namespace Duck.Content;

public class AssetDatabase
{
    private readonly ConcurrentDictionary<Guid, IAsset> _assetsByGuid = new();
    private readonly ConcurrentDictionary<string, Guid> _guidByUri = new();

    public AssetDatabase()
    {
        Register(new Texture2DAsset(new Uri("file:///Content/PolygonPrototype/Textures/PolygonPrototype_Texture_Grid_01.png")));
        Register(new Texture2DAsset(new Uri("file:///Content/PolygonPrototype/Textures/PolygonPrototype_Texture_Grid_08.png")));
        Register(new MeshAsset(new Uri("file:///Content/PolygonPrototype/StaticMeshes/SM_Pawn_Run_Male_01.mesh")));
        Register(new MeshAsset(new Uri("file:///Content/PolygonPrototype/StaticMeshes/SM_Buildings_Floor_1x1_01.mesh")));
        Register(new MaterialAsset(new Uri("file:///Content/PolygonPrototype/Materials/PolygonPrototype_Global.filamat")));

        var materialInstance = new MaterialInstanceAsset(new Uri("memory:///PolygonPrototype_Global_Grid_06"), GetReference<IMaterialAsset>(new Uri("file:///Content/PolygonPrototype/Materials/PolygonPrototype_Global.filamat")));
        // materialInstance.SetParameter("baseColor", RgbType.sRgb, new Color(0.5058824f, 0.5333334f, 0.5647059f));
        materialInstance.SetParameter("albedo", GetReference<ITexture2DAsset>(new Uri("file:///Content/PolygonPrototype/Textures/PolygonPrototype_Texture_Grid_08.png")));
        Register(materialInstance);

        materialInstance = new MaterialInstanceAsset(new Uri("memory:///PolygonPrototype_Global_Grid_04"), GetReference<IMaterialAsset>(new Uri("file:///Content/PolygonPrototype/Materials/PolygonPrototype_Global.filamat")));
        // materialInstance.SetParameter("baseColor", RgbType.sRgb, new Color(0.7426471f, 0.264421f, 0.2020437f));
        materialInstance.SetParameter("albedo", GetReference<ITexture2DAsset>(new Uri("file:///Content/PolygonPrototype/Textures/PolygonPrototype_Texture_Grid_01.png")));
        Register(materialInstance);
    }

    private void Register(IAsset asset)
    {
        if (_guidByUri.ContainsKey(asset.Uri.ToString())) {
            throw new Exception("FIXME: asset already registered");
        }

        _assetsByGuid.AddOrUpdate(asset.Id, asset, (g, s) => asset);
        _guidByUri.AddOrUpdate(asset.Uri.ToString(), asset.Id, (s, g) => asset.Id);
    }

    public AssetReference<T> GetReference<T>(Uri uri) where T : IAsset
    {
        if (!_guidByUri.ContainsKey(uri.ToString())) {
            throw new Exception("FIXME: asset doesn't exist: " + uri.ToString());
        }

        _guidByUri.TryGetValue(uri.ToString(), out var id);

        return new AssetReference<T>(id);
    }

    public T GetAsset<T>(AssetReference<T> assetReference) where T : IAsset
    {
        if (!_assetsByGuid.TryGetValue(assetReference.Id, out var asset)) {
            throw new Exception("FIXME: unknown asset");
        }

        return (T)asset;
    }
}

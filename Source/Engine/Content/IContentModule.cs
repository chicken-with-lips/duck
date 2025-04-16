using System.Runtime.CompilerServices;
using Arch.Core;
using Duck.Platform;

namespace Duck.Content;

public interface IContentModule : IModule
{
    public IAssetDatabase Database { get; }
    public string ContentRootDirectory { get; set; }
    public bool ReloadChangedContent { get; set; }

    public IContentModule RegisterSourceAssetImporter(ISourceAssetImporter importer);

    public ISourceAssetImporter<TAsset>? FindSourceAssetImporter<TAsset>(string file)
        where TAsset : class, IAsset;

    public TAsset? Import<TAsset>(string file)
        where TAsset : class, IAsset;

    public IContentModule RegisterAssetLoader<T, U>(IAssetLoader loader)
        where T : class, IAsset
        where U : class, IPlatformAsset;

    public IAssetLoader? FindAssetLoader<T>(T asset, IAssetLoadContext context)
        where T : class, IAsset;

    public IPlatformAsset LoadImmediate<T>(AssetReference<T> assetReference, IAssetLoadContext context, byte[]? fixmeData = null)
        where T : class, IAsset;

    public IPlatformAsset LoadImmediate<T>(AssetReference<T> asset)
        where T : class, IAsset;

    bool IsLoaded<T>(AssetReference<T> assetReference)
        where T : class, IAsset;
}

public readonly struct AssetReference<T>
    where T : class, IAsset
{
    public Guid AssetId { get; init; }

    public Guid UniqueId { get; init; }
    public bool IsShared { get; init; }

    public static readonly AssetReference<T> Null = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(AssetReference<T> other)
    {
        return AssetId.Equals(other.AssetId)
               && UniqueId.Equals(other.UniqueId)
               && IsShared == other.IsShared;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        return obj is AssetReference<T> other && Equals(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(AssetReference<T> left, AssetReference<T> right)
    {
        return left.Equals(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(AssetReference<T> left, AssetReference<T> right)
    {
        return !left.Equals(right);
    }

    public static AssetReference<T> Shared(Guid assetId)
    {
        return new AssetReference<T>() {
            AssetId = assetId,
            IsShared = true,
        };
    }

    public static AssetReference<T> Unique(Guid assetId)
    {
        return new AssetReference<T>() {
            AssetId = assetId,
            UniqueId = Guid.NewGuid(),
            IsShared = false,
        };
    }
}

public interface IAssetLoadContext
{
}

public struct EmptyAssetLoadContext : IAssetLoadContext
{
    public static EmptyAssetLoadContext Default => new();
}

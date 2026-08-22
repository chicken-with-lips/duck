using System.Runtime.CompilerServices;

namespace Duck.Content;

public readonly struct AssetReference<T> : IEquatable<AssetReference<T>> where T : class, IAsset
{
    public Guid AssetId { get; init; }

    public Guid UniqueId { get; init; }
    public bool IsShared { get; init; }

    public static readonly AssetReference<T> Null = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return HashCode.Combine(AssetId, UniqueId, IsShared);
    }

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

    public static AssetReference<T> Shared(in Guid assetId)
    {
        return new AssetReference<T> {
            AssetId = assetId,
            IsShared = true,
        };
    }

    public static AssetReference<T> Unique(in Guid assetId)
    {
        return new AssetReference<T> {
            AssetId = assetId,
            UniqueId = Guid.NewGuid(),
            IsShared = false,
        };
    }
}
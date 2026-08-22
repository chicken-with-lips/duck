using System.Diagnostics;

namespace Duck.Content;

public readonly struct AssetUri : IEquatable<AssetUri>
{
    public string Scheme { get; }
    public string Path { get; }

    public AssetUri(string scheme, string path)
    {
        Debug.Assert(!string.IsNullOrEmpty(scheme));
        Debug.Assert(!string.IsNullOrEmpty(path));

        Scheme = scheme;
        Path = path;
    }

    public static AssetUri Parse(string uri)
    {
        var parts = uri.Split("://");

        if (parts.Length != 2) {
            throw new MalformedAssetUri(uri);
        }

        return new AssetUri(parts[0], parts[1]);
    }
    public bool Equals(AssetUri other)
    {
        return Scheme == other.Scheme && Path == other.Path;
    }

    public override bool Equals(object? obj)
    {
        return obj is AssetUri other && Equals(other);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Scheme, Path);
    }
}

public class MalformedAssetUri : ApplicationException
{
    public string Uri { get; }

    public MalformedAssetUri(string uri)
        : base("Asset URI is malformed")
    {
        Uri = uri;
    }
}
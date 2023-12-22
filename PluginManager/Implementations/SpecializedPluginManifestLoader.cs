using System.IO;
using OneOf;
using OneOf.Types;

namespace Shockah.PluginManager;

public sealed class SpecializedPluginManifestLoader<TSpecializedPluginManifest, TPluginManifest> : IPluginManifestLoader<TPluginManifest>
    where TSpecializedPluginManifest : TPluginManifest
{
    private IPluginManifestLoader<TSpecializedPluginManifest> ManifestLoader { get; init; }

    public SpecializedPluginManifestLoader(IPluginManifestLoader<TSpecializedPluginManifest> manifestLoader)
    {
        this.ManifestLoader = manifestLoader;
    }

    public OneOf<TPluginManifest, Error<string>> LoadPluginManifest(Stream stream)
        => this.ManifestLoader.LoadPluginManifest(stream).Match<OneOf<TPluginManifest, Error<string>>>(
            manifest => manifest,
            error => error
        );
}

using System.Collections.Generic;
using Shockah.PluginManager;

namespace Nickel;

public interface IModManifest
{
    string UniqueName { get; }

    SemanticVersion Version { get; }

    SemanticVersion RequiredApiVersion { get; }

    IReadOnlySet<PluginDependency> Dependencies { get; }

    IReadOnlyDictionary<string, object> ExtensionData { get; }
}

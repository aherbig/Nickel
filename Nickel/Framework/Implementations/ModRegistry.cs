using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Nanoray.Pintail;
using Nickel.Common;

namespace Nickel;

internal sealed class ModRegistry : IModRegistry
{
    public IReadOnlyDictionary<string, IModManifest> LoadedMods
        => this.ModUniqueNameToInstance
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Package.Manifest);

    private IModManifest ModManifest { get; }
    internal IReadOnlyDictionary<string, Mod> ModUniqueNameToInstance { get; }
    private Dictionary<string, object?> ApiCache { get; } = new();
    private IProxyManager<string> ProxyManager { get; }

    public ModRegistry(IModManifest modManifest, IReadOnlyDictionary<string, Mod> modUniqueNameToInstance)
    {
        this.ModManifest = modManifest;
        this.ModUniqueNameToInstance = modUniqueNameToInstance;

        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName($"{GetType().Namespace}.Proxies, Version={this.GetType().Assembly.GetName().Version}, Culture=neutral"), AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule($"{GetType().Namespace}.Proxies");
        this.ProxyManager = new ProxyManager<string>(moduleBuilder, new ProxyManagerConfiguration<string>(
            proxyPrepareBehavior: ProxyManagerProxyPrepareBehavior.Eager,
            accessLevelChecking: AccessLevelChecking.DisabledButOnlyAllowPublicMembers
        ));
    }

    public bool TryProxy<TProxy>(object @object, [MaybeNullWhen(false)] out TProxy proxy) where TProxy : class
        => this.ProxyManager.TryProxy(@object, "TryProxy", this.ModManifest.UniqueName, out proxy);

    public TProxy? AsProxy<TProxy>(object? @object) where TProxy : class
        => @object is not null && this.TryProxy<TProxy>(@object, out var proxy) ? proxy : null;

    public TApi? GetApi<TApi>(string uniqueName, SemanticVersion? minimumVersion = null) where TApi : class
    {
        if (!typeof(TApi).IsInterface)
            throw new ArgumentException($"The requested API type {typeof(TApi)} is not an interface.");
        if (!this.ModUniqueNameToInstance.TryGetValue(uniqueName, out var mod))
            return null;
        if (minimumVersion is not null && minimumVersion > mod.Package.Manifest.Version)
            return null;

        if (!this.ApiCache.TryGetValue(uniqueName, out object? apiObject))
        {
            apiObject = mod.GetApi(this.ModManifest);
            this.ApiCache[uniqueName] = apiObject;
        }
        if (apiObject is null)
            throw new ArgumentException($"The mod {uniqueName} does not expose an API.");

        return this.ProxyManager.ObtainProxy<string, TApi>(apiObject, uniqueName, this.ModManifest.UniqueName);
    }
}

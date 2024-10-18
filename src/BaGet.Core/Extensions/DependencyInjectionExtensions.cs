using BaGet.Core.Authentication;
using BaGet.Core.Extensions;
using BaGet.Core.Search;
using BaGet.Core.Upstream;
using BaGet.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using BaGet.Core.Indexing;

namespace BaGet.Core
{
    public static partial class DependencyInjectionExtensions
    {
        //public static IServiceCollection AddBaGetApplication(
        //    this IServiceCollection services,
        //    Action<BaGetApplication> configureAction)
        //{
        //    var app = new BaGetApplication(services);
        //    services.AddBaGetServices();

        //    configureAction(app);

        //    return services;
        //}

        //private static void AddBaGetServices(this IServiceCollection services)
        //{
        //    //services.TryAddSingleton(HttpClientFactory);
        //    //services.TryAddSingleton(NuGetClientFactoryFactory);

            

            
            
            
            
            
            
            
            

            
            
        //    //services.TryAddTransient<V2UpstreamClient>();
        //    //services.TryAddTransient<V3UpstreamClient>();
        //    //services.TryAddTransient<DisabledUpstreamClient>();

        //    //services.TryAddTransient(UpstreamClientFactory);
        //}

        //private static HttpClient HttpClientFactory(IServiceProvider provider)
        //{
        //    var options = provider.GetRequiredService<IOptions<MirrorOptions>>().Value;

        //    var assembly = Assembly.GetEntryAssembly();
        //    var assemblyName = assembly!.GetName().Name;
        //    var assemblyVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

        //    var client = new HttpClient(new HttpClientHandler
        //    {
        //        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        //    });

        //    client.DefaultRequestHeaders.Add("User-Agent", $"{assemblyName}/{assemblyVersion}");
        //    client.Timeout = TimeSpan.FromSeconds(options.PackageDownloadTimeoutSeconds);

        //    return client;
        //}

        //private static NuGetClientFactory NuGetClientFactoryFactory(IServiceProvider provider)
        //{
        //    var httpClient = provider.GetRequiredService<HttpClient>();
        //    var options = provider.GetRequiredService<IOptions<MirrorOptions>>();

        //    return new NuGetClientFactory(
        //        httpClient,
        //        options.Value.PackageSource.ToString());
        //}

        //private static IUpstreamClient UpstreamClientFactory(IServiceProvider provider)
        //{
        //    var options = provider.GetRequiredService<IOptions<MirrorOptions>>();

        //    // TODO: Convert to switch expression.
        //    if (!options.Value.Enabled)
        //    {
        //        return provider.GetRequiredService<DisabledUpstreamClient>();
        //    }

        //    else if (options.Value.Legacy)
        //    {
        //        return provider.GetRequiredService<V2UpstreamClient>();
        //    }

        //    else
        //    {
        //        return provider.GetRequiredService<V3UpstreamClient>();
        //    }
        //}
    }
}

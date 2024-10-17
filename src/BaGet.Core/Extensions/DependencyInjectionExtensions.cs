using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using BaGet.Core.Authentication;
using BaGet.Core.Configuration;
using BaGet.Core.Extensions;
using BaGet.Core.Search;
using BaGet.Core.Upstream;
using BaGet.Protocol;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace BaGet.Core
{
    public static partial class DependencyInjectionExtensions
    {
        public static IServiceCollection AddBaGetApplication(
            this IServiceCollection services,
            Action<BaGetApplication> configureAction)
        {
            var app = new BaGetApplication(services);

            //services.AddConfiguration();
            services.AddBaGetServices();
            services.AddDefaultProviders();

            configureAction(app);

            return services;
        }

        /// <summary>
        /// Configures and validates options.
        /// </summary>
        /// <typeparam name="TOptions">The options type that should be added.</typeparam>
        /// <param name="services">The dependency injection container to add options.</param>
        /// <param name="key">
        /// The configuration key that should be used when configuring the options.
        /// If null, the root configuration will be used to configure the options.
        /// </param>
        /// <returns>The dependency injection container.</returns>
        public static IServiceCollection AddBaGetOptions<TOptions>(
            this IServiceCollection services,
            string? key = null)
            where TOptions : class
        {
            services.AddSingleton<IValidateOptions<TOptions>>(new ValidateBaGetOptions<TOptions>(key));
            services.AddSingleton<IConfigureOptions<TOptions>>(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                if (key != null)
                {
                    config = config.GetSection(key);
                }

                return new BindOptions<TOptions>(config);
            });

            return services;
        }

        private static void AddConfiguration(this IServiceCollection services)
        {
            services.AddBaGetOptions<BaGetOptions>();
            services.AddBaGetOptions<DatabaseOptions>(nameof(BaGetOptions.Database));
            services.AddBaGetOptions<FileSystemStorageOptions>(nameof(BaGetOptions.Storage));
            services.AddBaGetOptions<MirrorOptions>(nameof(BaGetOptions.Mirror));
            services.AddBaGetOptions<SearchOptions>(nameof(BaGetOptions.Search));
            services.AddBaGetOptions<StorageOptions>(nameof(BaGetOptions.Storage));
        }

        private static void AddBaGetServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IFrameworkCompatibilityService, FrameworkCompatibilityService>();
            services.TryAddSingleton<IPackageDownloadsSource, PackageDownloadsJsonSource>();

            services.TryAddSingleton<ISearchResponseBuilder, SearchResponseBuilder>();
            services.TryAddSingleton<NuGetClient>();
            services.TryAddSingleton<RegistrationBuilder>();
            services.TryAddSingleton<SystemTime>();
            services.TryAddSingleton<ValidateStartupOptions>();

            services.TryAddSingleton(HttpClientFactory);
            services.TryAddSingleton(NuGetClientFactoryFactory);

            services.TryAddScoped<DownloadsImporter>();

            services.TryAddTransient<IAuthenticationService, ApiKeyAuthenticationService>();
            services.TryAddTransient<IPackageContentService, DefaultPackageContentService>();
            services.TryAddTransient<IPackageDeletionService, PackageDeletionService>();
            services.TryAddTransient<IPackageIndexingService, PackageIndexingService>();
            services.TryAddTransient<IPackageMetadataService, DefaultPackageMetadataService>();
            services.TryAddTransient<IPackageService, PackageService>();
            services.TryAddTransient<IPackageStorageService, PackageStorageService>();
            services.TryAddTransient<IServiceIndexService, BaGetServiceIndex>();
            services.TryAddTransient<ISymbolIndexingService, SymbolIndexingService>();
            services.TryAddTransient<ISymbolStorageService, SymbolStorageService>();

            services.TryAddTransient<DatabaseSearchService>();
            services.TryAddTransient<FileStorageService>();
            services.TryAddTransient<PackageService>();
            services.TryAddTransient<V2UpstreamClient>();
            services.TryAddTransient<V3UpstreamClient>();
            services.TryAddTransient<DisabledUpstreamClient>();
            services.TryAddTransient<PackageDatabase>();

            services.TryAddTransient(UpstreamClientFactory);
        }

        private static void AddDefaultProviders(this IServiceCollection services)
        {
            services.AddProvider<IStorageService>((provider, configuration) =>
            {
                if (configuration.HasStorageType("filesystem"))
                {
                    return provider.GetRequiredService<FileStorageService>();
                }

                return null;
            });
        }

        private static HttpClient HttpClientFactory(IServiceProvider provider)
        {
            var options = provider.GetRequiredService<IOptions<MirrorOptions>>().Value;

            var assembly = Assembly.GetEntryAssembly();
            var assemblyName = assembly!.GetName().Name;
            var assemblyVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

            var client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            });

            client.DefaultRequestHeaders.Add("User-Agent", $"{assemblyName}/{assemblyVersion}");
            client.Timeout = TimeSpan.FromSeconds(options.PackageDownloadTimeoutSeconds);

            return client;
        }

        private static NuGetClientFactory NuGetClientFactoryFactory(IServiceProvider provider)
        {
            var httpClient = provider.GetRequiredService<HttpClient>();
            var options = provider.GetRequiredService<IOptions<MirrorOptions>>();

            return new NuGetClientFactory(
                httpClient,
                options.Value.PackageSource.ToString());
        }

        private static IUpstreamClient UpstreamClientFactory(IServiceProvider provider)
        {
            var options = provider.GetRequiredService<IOptions<MirrorOptions>>();

            // TODO: Convert to switch expression.
            if (!options.Value.Enabled)
            {
                return provider.GetRequiredService<DisabledUpstreamClient>();
            }

            else if (options.Value.Legacy)
            {
                return provider.GetRequiredService<V2UpstreamClient>();
            }

            else
            {
                return provider.GetRequiredService<V3UpstreamClient>();
            }
        }
    }
}

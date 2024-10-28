using BaGet.Core;
using BaGet.Core.Authentication;
using BaGet.Core.Configuration;
using BaGet.Core.Content;
using BaGet.Core.Entities;
using BaGet.Core.Indexing;
using BaGet.Core.Metadata;
using BaGet.Core.Search;
using BaGet.Core.Storage;
using BaGet.Core.Upstream;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Text;
using System.Text.Json.Serialization;
using Xanadu.Skidbladnir.IO.File.Cache;

namespace BaGet
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var applicationBuilder = WebApplication.CreateBuilder(args);

                applicationBuilder.Services.AddLogging(builder => builder.AddSerilog());
                // 注册日志文件输出，48MB大小限制
                applicationBuilder.Services.AddSerilog(configuration => configuration
                    .Enrich.FromLogContext()
                    .WriteTo.File("BaGet.log", encoding: Encoding.UTF8, fileSizeLimitBytes: 50331648));

                // Add Options to bind configuration.
                applicationBuilder.Services.AddOptions<BaGetOptions>().Bind(applicationBuilder.Configuration).ValidateDataAnnotations();
                applicationBuilder.Services.AddOptions<DatabaseOptions>().Bind(applicationBuilder.Configuration.GetSection(nameof(BaGetOptions.Database)))
                    .ValidateDataAnnotations();
                applicationBuilder.Services.AddOptions<StorageOptions>().Bind(applicationBuilder.Configuration.GetSection(nameof(BaGetOptions.Storage)))
                    .ValidateDataAnnotations();
                applicationBuilder.Services.AddOptions<SearchOptions>().Bind(applicationBuilder.Configuration.GetSection(nameof(BaGetOptions.Search)))
                    .ValidateDataAnnotations();
                applicationBuilder.Services.AddOptions<FileSystemStorageOptions>()
                    .Bind(applicationBuilder.Configuration.GetSection(nameof(BaGetOptions.Storage))).ValidateDataAnnotations();
                // TODO: Reconstruct the Mirror feature because it is not cable to be used on HttpClient.
                // applicationBuilder.Services.AddOptions<MirrorOptions>().Bind(applicationBuilder.Configuration.GetSection(nameof(BaGetOptions.Mirror))).ValidateDataAnnotations();

                // Configure the forwarded headers middleware to forward the X-Forwarded-For and X-Forwarded-Proto headers.
                applicationBuilder.Services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.All;
                });

                // Add DbContext, using the database type specified in the configuration.
                applicationBuilder.Services.AddDbContext<BaGetDbContext>((provider, builder) =>
                {
                    var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                    _ = options.Type.ToLowerInvariant() switch
                    {
                        "mariadb" => builder.UseMySql(options.ConnectionString, ServerVersion.AutoDetect(options.ConnectionString),
                            x => x.MigrationsAssembly("BaGet.Database.Mariadb")),
                        "sqlite" => builder.UseSqlite(options.ConnectionString,
                            x => x.MigrationsAssembly("BaGet.Database.Sqlite")),
                        "sqlserver" => builder.UseSqlServer(options.ConnectionString,
                            x => x.MigrationsAssembly("BaGet.Database.SqlServer")),
                        "postgresql" => builder.UseNpgsql(options.ConnectionString,
                            x => x.MigrationsAssembly("BaGet.Database.PostgreSql")),
                        _ => throw new ArgumentOutOfRangeException(nameof(options.Type), "Unsupported Database!")
                    };
                }
                );

                // Add ASP.NET Core base services.
                applicationBuilder.Services.AddRazorPages();
                applicationBuilder.Services.AddControllers();
                applicationBuilder.Services.AddEndpointsApiExplorer();
                applicationBuilder.Services.AddSwaggerGen();
                applicationBuilder.Services.AddHttpContextAccessor();
                applicationBuilder.Services.ConfigureHttpJsonOptions(option =>
                {
                    option.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

                // Add Xanadu services.
                applicationBuilder.Services.AddSingleton<IFileCachePool, FileCachePool>();

                // Add BaGet services in ordered.
                applicationBuilder.Services.AddScoped<IPackageDatabase, PackageDatabase>();
                applicationBuilder.Services.AddHttpClient<IPackageDownloadsSource, PackageDownloadsJsonSource>();
                applicationBuilder.Services.AddSingleton<IFrameworkCompatibilityService, FrameworkCompatibilityService>();
                applicationBuilder.Services.AddTransient<IUrlGenerator, BaGetUrlGenerator>();
                applicationBuilder.Services.AddSingleton<ISearchResponseBuilder, SearchResponseBuilder>();
                applicationBuilder.Services.AddSingleton<RegistrationBuilder>();
                applicationBuilder.Services.AddScoped<DownloadsImporter>();
                applicationBuilder.Services.AddTransient<IAuthenticationService, ApiKeyAuthenticationService>();
                applicationBuilder.Services.AddSingleton<IStorageService, FileStorageService>();
                applicationBuilder.Services.AddTransient<IPackageStorageService, PackageStorageService>();
                applicationBuilder.Services.AddTransient<IPackageContentService, DefaultPackageContentService>();
                applicationBuilder.Services.AddTransient<IPackageDeletionService, PackageDeletionService>();
                applicationBuilder.Services.AddTransient<IPackageIndexingService, PackageIndexingService>();
                applicationBuilder.Services.AddTransient<IPackageService, PackageService>();
                applicationBuilder.Services.AddTransient<IPackageMetadataService, DefaultPackageMetadataService>();
                applicationBuilder.Services.AddTransient<IServiceIndexService, BaGetServiceIndex>();
                applicationBuilder.Services.AddTransient<ISymbolStorageService, SymbolStorageService>();
                applicationBuilder.Services.AddTransient<ISymbolIndexingService, SymbolIndexingService>();
                applicationBuilder.Services.AddTransient<ISearchService, DatabaseSearchService>();

                applicationBuilder.Services.AddCors(options =>
                {
                    options.AddPolicy("DevelopCors", policy =>
                    {
                        policy.AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowAnyOrigin();
                    });
                });

                var app = applicationBuilder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<BaGetDbContext>();
                    db.Database.Migrate();
                }

                var options = app.Configuration.Get<BaGetOptions>()!;

                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseStatusCodePages();
                    app.UseCors("DevelopCors");
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }
                else
                {
                    app.UseExceptionHandler("/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseForwardedHeaders();
                app.UsePathBase(options.PathBase);

                //app.UseOperationCancelledMiddleware();

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();

                app.MapRazorPages();
                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "BaGet failed to start");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }

            return 0;
        }
    }
}

using BaGet;
using BaGet.Core;
using BaGet.Core.Configuration;
using BaGet.Core.Entities;
using BaGet.Core.Extensions;
using BaGet.Database.Mariadb;
using BaGet.Database.Sqlite;
using BaGet.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using DependencyInjectionExtensions = BaGet.Core.Extensions.DependencyInjectionExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add Options to bind configuration.
builder.Services.AddOptions<BaGetOptions>().Bind(builder.Configuration).ValidateDataAnnotations();
builder.Services.AddOptions<DatabaseOptions>().Bind(builder.Configuration.GetSection(nameof(BaGetOptions.Database)))
    .ValidateDataAnnotations();
builder.Services.AddOptions<StorageOptions>().Bind(builder.Configuration.GetSection(nameof(BaGetOptions.Storage)))
    .ValidateDataAnnotations();
builder.Services.AddOptions<SearchOptions>().Bind(builder.Configuration.GetSection(nameof(BaGetOptions.Search)))
    .ValidateDataAnnotations();
builder.Services.AddOptions<FileSystemStorageOptions>()
    .Bind(builder.Configuration.GetSection(nameof(BaGetOptions.Storage))).ValidateDataAnnotations();
builder.Services.AddOptions<MirrorOptions>().Bind(builder.Configuration.GetSection(nameof(BaGetOptions.Mirror)))
    .ValidateDataAnnotations();

// Configure the forwarded headers middleware to forward the X-Forwarded-For and X-Forwarded-Proto headers.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
});


builder.Services.AddDbContext<BaGetDbContext>((provider, builder) =>
{
    var databaseOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    switch (databaseOptions.Type.ToLowerInvariant()) // 将 Type 转为小写以忽略大小写
    {
        case "mariadb":
            builder.UseMySql(databaseOptions.ConnectionString,
                ServerVersion.AutoDetect(databaseOptions.ConnectionString));
            break;
        case "sqlite":
            builder.UseSqlite(databaseOptions.ConnectionString);
            break;
        default:
            throw new InvalidOperationException($"Unsupported database type: {databaseOptions.Type}");
    }
});

builder.Services.AddBaGetWebApplication(app =>
{

    // Add storage providers.
    app.AddFileStorage();

    // Add search providers.
});

// You can swap between implementations of subsystems like storage and search using BaGet's configuration.
// Each subsystem's implementation has a provider that reads the configuration to determine if it should be
// activated. BaGet will run through all its providers until it finds one that is active.
builder.Services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<IStorageService>);
builder.Services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<IPackageDatabase>);
builder.Services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<ISearchService>);
builder.Services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<ISearchIndexer>);
builder.Services.AddTransient<IUrlGenerator, BaGetUrlGenerator>();
builder.Services.AddRazorPages();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors();



var app = builder.Build();
var options = app.Configuration.Get<BaGetOptions>()!;

// Add storage providers.


// Add search providers.

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseStatusCodePages();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UsePathBase(options.PathBase);

app.UseStaticFiles();
app.UseRouting();

app.UseOperationCancelledMiddleware();

app.UseEndpoints(endpoints =>
{
    var baget = new BaGetEndpointBuilder();

    baget.MapEndpoints(endpoints);
});

public partial class Program;

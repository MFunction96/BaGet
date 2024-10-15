using BaGet;
using BaGet.Core;
using BaGet.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
});
builder.Services.AddBaGetWebApplication(app =>
{
    // Add database providers.
    app.AddMariadbDatabase();
    app.AddSqliteDatabase();

    // Add storage providers.
    app.AddFileStorage();

    // Add search providers.
});

// You can swap between implementations of subsystems like storage and search using BaGet's configuration.
// Each subsystem's implementation has a provider that reads the configuration to determine if it should be
// activated. BaGet will run through all its providers until it finds one that is active.
builder.Services.AddScoped(DependencyInjectionExtensions.GetServiceFromProviders<IContext>);
builder.Services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<IStorageService>);
builder.Services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<IPackageDatabase>);
builder.Services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<ISearchService>);
builder.Services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<ISearchIndexer>);
builder.Services.AddRazorPages();
builder.Services.AddCors();

var app = builder.Build();
var options = app.Configuration.Get<BaGetOptions>()!;

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

public partial class Program { }

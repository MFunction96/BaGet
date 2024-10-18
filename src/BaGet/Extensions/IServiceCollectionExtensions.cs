using System;
using System.Text.Json.Serialization;
using BaGet.Controllers;
using BaGet.Core;
using BaGet.Web;
using Microsoft.Extensions.DependencyInjection;

namespace BaGet.Extensions
{
    public static class ServiceCollectionExtensions
    {
        //public static IServiceCollection AddBaGetWebApplication(
        //    this IServiceCollection services,
        //    Action<BaGetApplication> configureAction)
        //{
        //    services
        //        .AddRouting(options => options.LowercaseUrls = true)
        //        .AddControllers()
        //        .AddApplicationPart(typeof(PackageContentController).Assembly)
        //        .AddJsonOptions(options =>
        //        {
        //            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        //        });

        //    services.AddHttpContextAccessor();
        //    services.AddTransient<IUrlGenerator, BaGetUrlGenerator>();

        //    services.AddBaGetApplication(configureAction);

        //    return services;
        //}
    }
}

using System;
using BaGet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BaGet
{
    public static class BaGetApplicationExtensions
    {
        public static BaGetApplication AddFileStorage(this BaGetApplication app)
        {
            app.Services.TryAddTransient<IStorageService>(provider => provider.GetRequiredService<FileStorageService>());
            return app;
        }

        public static BaGetApplication AddFileStorage(
            this BaGetApplication app,
            Action<FileSystemStorageOptions> configure)
        {
            app.AddFileStorage();
            app.Services.Configure(configure);
            return app;
        }
    }
}

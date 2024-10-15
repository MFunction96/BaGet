using System;
using BaGet.Web;
using Microsoft.AspNetCore.Builder;

namespace BaGet.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseOperationCancelledMiddleware(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            return app.UseMiddleware<OperationCancelledMiddleware>();
        }
    }
}

using BestPractices.Correlation.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BestPractices.Correlation.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context => {
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    logger.LogException(contextFeature.Error);

                    // either throw the same exception to the calling method 
                    throw contextFeature.Error; 
                    // or construct a valid error response to return.
                    //await context.Response.WriteAsync("500 - Internal Server Error");
                });
            });
        }
    }
}

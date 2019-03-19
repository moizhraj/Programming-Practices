using BestPractices.Correlation.Contracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BestPractices.Correlation.helpers
{
    public class CustomHttpMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public CustomHttpMiddleware(RequestDelegate _next, ILogger _logger)
        {
            next = _next;
            logger = _logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // add a correlationId if doesn't exist which can be reused in next handle
                if (!context.Request.Headers.ContainsKey("x-correlation-id"))
                    context.Request.Headers.Add("x-correlation-id", System.Guid.NewGuid().ToString());
                context.Response.Headers.Add("x-correlation-id", context.Request.Headers["x-correlation-id"]);

                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // Log the exception 
            logger.LogException(ex);

            // then, either handle the error 
            //context.Response.ContentType = "application/json";
            //context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            //return context.Response.WriteAsync($"{(int)HttpStatusCode.InternalServerError} - Internal Server Error");

            // or just throw it to the calling method
            throw ex;
        }
    }
}

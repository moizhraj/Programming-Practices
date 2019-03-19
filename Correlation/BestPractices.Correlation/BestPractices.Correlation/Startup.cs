using BestPractices.Correlation.Contracts;
using BestPractices.Correlation.Extensions;
using BestPractices.Correlation.helpers;
using BestPractices.Correlation.Models;
using BestPractices.Correlation.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BestPractices.Correlation
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("x-correlation-id");
                    });
            });
            services.AddSingleton(Configuration);
            services.Configure<AppConfig>(Configuration);
            // OOB
            services.AddSingleton(typeof(IHttpContextAccessor), typeof(HttpContextAccessor));
            // Custom
            services.AddSingleton(typeof(ILogger), typeof(AiLogger));
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.ConfigureExceptionHandler(logger);          // Use Exception Handler
            //app.UseMiddleware<CustomHttpMiddleware>();    // Or use Custom Middle ware to log events and exceptions
            //app.Use(async (context, next) =>              // or use in built middleware
            //{
            //    // add a correlationId if doesn't exist which can be reused in next handle
            //    if (!context.Request.Headers.ContainsKey("x-correlation-id"))
            //        context.Request.Headers.Add("x-correlation-id", System.Guid.NewGuid().ToString());
            //    context.Response.Headers.Add("x-correlation-id", context.Request.Headers["x-correlation-id"]);
            //    await next();
            //});
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}

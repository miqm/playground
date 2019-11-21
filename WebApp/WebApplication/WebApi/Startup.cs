using System;
using System.Diagnostics;
using AutoMapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Miqm.WebApi
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
            services.AddControllers().SetCompatibilityVersion(CompatibilityVersion.Version_3_0).AddFluentValidation(fv =>
            {
                fv.RegisterValidatorsFromAssemblyContaining<Startup>();
            });
            services.AddAutoMapper(typeof(Startup));
            services.AddSwaggerGen(c =>
            {
                c.OperationFilter<CustomResponseType>();
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1",
                    Contact = new OpenApiContact()
                    {
                        Email = "test@example.com",
                        Url = new Uri("https://example.com"),
                        Name = "Test Name"              
                    }
                });
            });
            services.Configure<ApiBehaviorOptions>(o =>
            {
                o.InvalidModelStateResponseFactory = (context) =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = o.ClientErrorMapping[StatusCodes.Status400BadRequest].Link,
                    };

                    SetTraceId(context, problemDetails);

                    var result = new BadRequestObjectResult(problemDetails);

                    result.ContentTypes.Add("application/problem+json");

                    return result;
                };
            });
        }
        internal static void SetTraceId(ActionContext actionContext, ProblemDetails problemDetails)
        {
            var traceId = Activity.Current?.Id ?? actionContext.HttpContext.TraceIdentifier;
            problemDetails.Extensions["traceId"] = traceId;
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
            app.UseRouting();

            app.UseSwagger(s => { 
            });

            app.UseSwaggerUI(c =>
            {                          
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            //app.UseHttpsRedirection();
            app.UseEndpoints(s => 
                s.MapControllers()
            );
        }
    }
}

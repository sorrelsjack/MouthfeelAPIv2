using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MouthfeelAPIv2.DbModels;
using MouthfeelAPIv2.Models;
using MouthfeelAPIv2.Services;
using Newtonsoft.Json;

namespace MouthfeelAPIv2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MouthfeelContext>(opt => opt.UseSqlServer(Configuration["DatabaseConnectionString"]), ServiceLifetime.Transient);
            services.AddScoped<IFoodsService, FoodsService>();
            services.AddScoped<IIngredientsService, IngredientsService>();
            services.AddScoped<IFlavorsService, FlavorsService>();
            services.AddScoped<ITexturesService, TexturesService>();
            services.AddScoped<IMiscellaneousService, MiscellaneousService>();
            services.AddScoped<ICommentsService, CommentsService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseExceptionHandler(new ExceptionHandlerOptions 
            {
                ExceptionHandler = context =>
                {
                    var response = context.Response;
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    var ex = exception as ErrorResponse;
                    response.StatusCode = ex.ErrorCode != null ? (int)ex.ErrorCode : (int)HttpStatusCode.InternalServerError;
                    var body = ex.ErrorMessage;

                    return response.WriteAsync(JsonConvert.SerializeObject(new { ErrorCode = response.StatusCode, Message = body }, Formatting.Indented));
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
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
            ConfigureConstants();
            IdentityModelEventSource.ShowPII = true;

            services.AddDbContext<MouthfeelContext>(opt => opt.UseSqlServer(Configuration["DatabaseConnectionString"]), ServiceLifetime.Transient);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = context =>
                    { 
                        Debug.WriteLine(context.Exception.Message);
                        return context.Response.WriteAsync(context.Exception.Message);
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    SaveSigninToken = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSigningSecret"]))
                };
            });
            services.AddScoped<IFoodsService, FoodsService>();
            services.AddScoped<IIngredientsService, IngredientsService>();
            services.AddScoped<IFlavorsService, FlavorsService>();
            services.AddScoped<ITexturesService, TexturesService>();
            services.AddScoped<IMiscellaneousService, MiscellaneousService>();
            services.AddScoped<ICommentsService, CommentsService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddControllers();
        }

        public Exception GetInnermostException(HttpContext context)
        {
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            while (exception.InnerException != null) exception = exception.InnerException;
            return exception;
        }

        public void ConfigureConstants()
        {
            MouthfeelApiConfiguration.JwtSigningSecret = Configuration["JwtSigningSecret"];
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseExceptionHandler(new ExceptionHandlerOptions 
            {
                ExceptionHandler = context =>
                {
                    try
                    {
                        var response = context.Response;
                        var exception = GetInnermostException(context);
                        var ex = exception as ErrorResponse;
                        response.StatusCode = ex?.ErrorCode != null ? (int)ex.ErrorCode : (int)HttpStatusCode.InternalServerError;

                        var body = ex.ErrorMessage;

                        response.ContentType = "application/json";
                        return response.WriteAsync(JsonConvert.SerializeObject(new { ErrorCode = response.StatusCode, DescriptiveErrorCode = ex.DescriptiveErrorCode, Message = body }, Formatting.Indented));
                    }
                    catch (Exception)
                    {
                        return context.Response.WriteAsync("An unexpected error occurred.");
                    }
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

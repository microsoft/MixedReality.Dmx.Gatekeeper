// ---------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------

using System.Text.Json.Serialization;
using DMX.Gatekeeper.Api.Brokers.DmxApis;
using DMX.Gatekeeper.Api.Brokers.Loggings;
using DMX.Gatekeeper.Api.Models.Configurations;
using DMX.Gatekeeper.Api.Securities.Policies;
using DMX.Gatekeeper.Api.Services.Foundations.Labs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

namespace DMX.Gatekeeper.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration) =>
            Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddHttpClient();

            AddAuthorization(services, this.Configuration);
            AddAuthentication(services, this.Configuration);
            AddBrokers(services);
            AddServices(services);

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(
                    name: "v1",
                    info: new OpenApiInfo
                    {
                        Title = "DMX.Gatekeeper.API",
                        Version = "v1"
                    }
                );
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint(
                        url: "/swagger/v1/swagger.json",
                        name: "DMX.Gatekeeper.API v1");
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            MapControllersForEnvironments(app, env);
        }

        private static void AddBrokers(IServiceCollection services)
        {
            services.AddTransient<ILoggingBroker, LoggingBroker>();
            services.AddTransient<IDmxApiBroker, DmxApiBroker>();
        }

        private static void AddServices(IServiceCollection services) =>
            services.AddTransient<ILabService, LabService>();
        
        private static void AddAuthorization(
            IServiceCollection services, IConfiguration configuration)
        {
            string[] readOnlyScopes = GetScopesFromConfiguration("AzureAd:Scopes:GetAllLabs", configuration);
            string[] readWriteScopes = GetScopesFromConfiguration("AzureAd:Scopes:PostLab", configuration);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ReadOnly", policy => policy.AddRequirements(
                    new HasScopeRequirement(readOnlyScopes)));

                options.AddPolicy("ReadWrite", policy => policy.AddRequirements(
                    new HasScopeRequirement(readWriteScopes)));
            });

            services.AddSingleton<IAuthorizationHandler, HasScopeRequirementHandler>();
        }

        private static void AddAuthentication(
            IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"))
                    .EnableTokenAcquisitionToCallDownstreamApi()
                        .AddDownstreamWebApi(
                            "DownstreamApi", configuration.GetSection("DownstreamApi"))
                                .AddInMemoryTokenCaches();
        }

        private static void MapControllersForEnvironments(
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            app.UseEndpoints(endpoints =>
            {
                if (env.IsDevelopment())
                {
                    endpoints.MapControllers().AllowAnonymous();
                }
                else
                {
                    endpoints.MapControllers();
                }
            });
        }
        private static string[] GetScopesFromConfiguration(
            string scopeConfigurationKey, IConfiguration configuration) =>
                configuration.GetValue<string>(scopeConfigurationKey)?.Split(' ');
    }
}
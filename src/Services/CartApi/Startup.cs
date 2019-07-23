using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CartApi.Infrastructure.Filters;
using CartApi.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Swagger;

namespace CartApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(opt => { opt.Filters.Add(typeof(HttpsGlobalExceptionFilter)); }
                ).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.Configure<CartSettings>(Configuration);

            services.AddSingleton<ConnectionMultiplexer>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<CartSettings>>();

                var connectionString = settings.Value.ConnectionString;

                if (HostingEnvironment.IsProduction())
                {
                    Console.WriteLine("Production");

                    var host = Configuration["RedisConnectionString"];
                    //var port = Configuration["RedisPort"];

                    Console.WriteLine($"Production - host: {host}");

                    connectionString = host ?? connectionString;
                    Console.WriteLine($"Production - connectionString: {connectionString}");
                    //!string.IsNullOrEmpty(host) && 
                    //!string.IsNullOrEmpty(port) 
                    //? $"{host}:{port}"
                    //: connectionString;
                }

                var configuration = ConfigurationOptions.Parse(connectionString, true);
                configuration.ResolveDns = true;
                configuration.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(configuration);
            });

            services.AddSwaggerGen(opt =>
                {
                    opt.DescribeAllEnumsAsStrings();
                    opt.SwaggerDoc("v1",new Info()
                    {
                        Title = "Basket HTTP API",
                        Version= "v1",
                        Description = "The Basket Service HTTP API",
                        TermsOfService = "Terms OF Service"
                    });
                });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IRedisRepository, RedisCartRepository>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseCors("CorsPolicy");
            app.UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/swagger/v1/swagger.json", "Basket.API V1");
                    //c.ConfigureOAuth2("basketswaggerui", "", "", "Basket Swagger UI");
                });

            app.UseMvcWithDefaultRoute();

        }
    }
}

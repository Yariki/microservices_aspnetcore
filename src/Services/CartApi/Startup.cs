using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CartApi.Infrastructure.Filters;
using CartApi.Model;
using Common.Messaging.Consumers;
using IdentityServer4.AccessTokenValidation;
using MassTransit;
using MassTransit.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;

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

        private IContainer ApplicationContainer { get; set; }

        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(opt => { opt.Filters.Add(typeof(HttpsGlobalExceptionFilter)); }
                )
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.Configure<CartSettings>(Configuration);

            ConfigureAuthService(services);
            
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
                    opt.AddSecurityDefinition("oauth2", new OAuth2Scheme()
                    {
                        Type = "oauth2",
                        Flow = "implicit",
                        AuthorizationUrl = $"{Configuration.GetValue<string>("IdentityUrl")}/connect/authorize",
                        
                        Scopes = new Dictionary<string, string>()
                        {
                            {"basket", "Basket Api"}
                        }
                    });

                    opt.OperationFilter<AuthotizeCheckOperationFilter>();
                });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    b => b.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IRedisRepository, RedisCartRepository>();


            var builder = new ContainerBuilder();

            builder.RegisterType<OrderCompletedEventConsumer>();

            builder.Register(c =>
                {
                    var busControl = Bus.Factory.CreateUsingRabbitMq(rmq =>
                    {
                        var host = rmq.Host(new Uri("rabbitmq://localhost"), "/", h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        rmq.ReceiveEndpoint(host, "ShoesContainers" + Guid.NewGuid().ToString(),
                            e => { e.LoadFrom(c); });

                    });

                    return busControl;
                })
                .SingleInstance()
                .As<IBusControl>()
                .As<IBus>();
                
            builder.Populate(services);
            ApplicationContainer = builder.Build();
            return new AutofacServiceProvider(ApplicationContainer);


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/swagger/v1/swagger.json", "Basket.API V1");
                    c.OAuthClientId("basketswaggerui");
                    c.OAuthAppName("Basket Swagger UI");

                });


            app.UseMvcWithDefaultRoute();

            var bus = ApplicationContainer.Resolve<IBusControl>();
            var busHandle = TaskUtil.Await(() => bus.StartAsync());
            applicationLifetime.ApplicationStopping.Register(() => busHandle.Stop());


        }

        private void ConfigureAuthService(IServiceCollection services)
        {
            var identityUrl = Configuration.GetValue<string>("IdentityUrl");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
                    opt =>
                    {
                        opt.Authority = identityUrl;
                        opt.RequireHttpsMetadata = false;
                        opt.Audience = "basket";
                    });
        }

    }
}

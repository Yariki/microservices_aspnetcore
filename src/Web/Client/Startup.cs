using System.IdentityModel.Tokens.Jwt;
using Client.Infrastructure;
using Client.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Client
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
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.Configure<AppSettings>(Configuration);

            services.AddSingleton<IHttpClient, CustomHttpClient>();
            services.AddTransient<ICatalogService, CatalogService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var identityUrl = Configuration.GetValue<string>("IdentityUrl");
            var callBackUrl = Configuration.GetValue<string>("CallBackUrl");
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                    options.Authority = identityUrl.ToString();
                    options.RequireHttpsMetadata = false;
                    //options.SignedOutRedirectUri = callBackUrl.ToString();

                    options.ClientId = "mvc";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code id_token";

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("offline_access");
                    options.Scope.Add("basket");
                    options.Scope.Add("orders");

                    options.ClaimActions.MapJsonKey("website", "website");
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Catalog/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Catalog}/{action=Index}/{id?}");
                routes.MapRoute(
                    "defaultError",
                    "{controller=Error}/{action=Error}");
            });
        }
    }
}
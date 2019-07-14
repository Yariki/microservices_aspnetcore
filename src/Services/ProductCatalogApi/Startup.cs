using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductCatalogApi.Data;
using Swashbuckle.AspNetCore.Swagger;

namespace ProductCatalogApi
{
    public class Startup
    {


        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        public IHostingEnvironment CurrentEnvironment { get; }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CatalogSettings>(Configuration);

            var connectionString = Configuration["ConnectionString"];

            if (CurrentEnvironment.IsProduction())
            {
                var server = Configuration["DatabaseServer"];
                var database = Configuration["DatabaseName"];
                var user = Configuration["DatabaseUser"];
                var password = Configuration["DatabaseNamePassword"];
                connectionString =
                    $"Server={server};Database={database};User Id={user};Password={password};MultipleActiveResultSets=true";
            }
            
            services.AddDbContext<CatalogContext>(opt => opt.UseSqlServer(connectionString));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSwaggerGen(opt =>
            {
                opt.DescribeAllEnumsAsStrings();
                opt.SwaggerDoc("v1", new Info()
                {
                    Title = "Product Container - Product Catalog HTTP API",
                    Version = "V1",
                    Description = "The Product Catalog Microservice HTTP API. This is Data driven/CRUD microservice sample",
                    TermsOfService = "Terms of Service"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger()
                .UseSwaggerUI(c => { c.SwaggerEndpoint($"/swagger/v1/swagger.json", "ProductCatalogAPI"); });
            app.UseMvc();
        }
    }
}

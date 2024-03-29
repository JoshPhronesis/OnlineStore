﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductCatalogApi.Data;
using ProductCatalogApi.Helpers;
using ShoesOnContainers.Services.ProductCatalogApi;
using Swashbuckle.AspNetCore.Swagger;

namespace ProductCatalogApi
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
            var server = Configuration["DatabaseServer"];
            var database = Configuration["DatabaseName"];
            var user = Configuration["DatabaseUser"];
            var password = Configuration["DatabaseUserPassword"];
            var connestionString = $"Server={server};Database={database};User={user};Password={password}";

            services.Configure<CatalogSettings>(Configuration);
            services.AddDbContext<CatalogContext>(options =>
            {
                options.UseSqlServer(connestionString);
            });
            //services.AddSwaggerGen()
            services.AddSwaggerGen(opt =>
            {
                opt.DescribeAllEnumsAsStrings();
                opt.SwaggerDoc("v1", new Info
                {
                    Title = "OnlineStore - Catalog API", Version = "v1",
                    Description = "Catalog Microservice API",
                });
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/swagger/v1/swagger.json", "CatalogAPI V1");
                });

            app.UseMvc();
        }
    }
}

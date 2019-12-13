using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnestesistaNew.SwaggerConfiguration
{
    public static class SwaggerConfiguration
    {
        public static void AddSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Zheus Calculos API", Version = "v1" });

                c.OperationFilter<DescriptionOperationFilter>(); // [Description] on Response properties

                c.OperationFilter<AddResponseHeadersFilter>(); // [SwaggerResponseHeader]

                c.CustomSchemaIds(x => x.FullName);
            });

        }

        public static void UseSwagger(this IApplicationBuilder app, IConfiguration configuration)
        {

            app.UseHsts();


            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";
                c.SwaggerEndpoint("v1/swagger.json", "Zheus Calculos API");
                c.DocExpansion(DocExpansion.None);
                c.EnableFilter();
            });
        }
    }
}

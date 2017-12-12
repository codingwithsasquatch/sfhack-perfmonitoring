using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace StatefulBackEnd
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(configuration.GetValue<string>("contentRoot"))
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{configuration.GetValue<string>("environment")}.json", optional: true)
                .AddInMemoryCollection(configuration.AsEnumerable())
                .AddEnvironmentVariables();
            this.Configuration = builder.Build();
            //Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddApplicationInsightsTelemetry("9395cd12-4756-4844-94a6-cda8d6ad032f");
            services.AddApplicationInsightsTelemetry(this.Configuration);
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}

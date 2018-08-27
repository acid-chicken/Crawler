using System.Net.Http;
using AngleSharp.Parser.Html;
using Crawler.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crawler
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
            services.AddSingleton<HtmlParser>(new HtmlParser());
            services.AddSingleton<Parser>();

            services.AddHttpClient("crawler", _ =>
                new HttpClient(new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                }));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            _ = env.IsDevelopment() ?
                app.UseDeveloperExceptionPage() :
                app.UseHsts();

            app.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}

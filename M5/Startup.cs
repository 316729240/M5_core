using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using M5.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MWMS.Helper;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace M5
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

           /* TemplateServiceConfiguration templateConfig = new TemplateServiceConfiguration
            {
                CatchPath = Tools.Mappath("assembly/")
            };
            Razor.SetTemplateService(new TemplateService(templateConfig));
            RazorEngine.Razor.Compile("kkkkkk@{Raw((3+4).ToString());}ffff", typeof(object[]), "test", true);
            string html = RazorEngine.Razor.Run("test", new object[] { "", "" });*/
           // return;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.Run(async (context) =>
            {
                SystemData.HttpContext = context;
                await MWMS_Rewrite(context);
            });
        }
        public static  async Task MWMS_Rewrite(HttpContext context)
        {

            TemplateServiceConfiguration templateConfig = new TemplateServiceConfiguration
            {
                CatchPath =Tools.Mappath("assembly/")
            };
            Razor.SetTemplateService(new TemplateService(templateConfig));
            RazorEngine.Razor.Compile("kkkkkk", typeof(object[]), "test", true);
            string html = RazorEngine.Razor.Run("test", new object[] { "","" });

            /*
            UrlMapping u1 = new UrlMapping();
            u1.Path = "2222";
            Uri url = new Uri(context.Request.Scheme+"://"+context.Request.Host+context.Request.Path);
            WebUri u = new WebUri()
            {
                MainMobileUrl="",
            };
            u.AddMapping(new UrlMapping()
            {
                Path = "/acf/",
                PcDomain = "/pc2/",
                MobileDomain = "/m/"
            });
            u.AddMapping(new UrlMapping() {
                    Path="/",
                    PcDomain="/",
                    MobileDomain="/m/"
            });
            u.AddMapping(new UrlMapping()
            {
                Path = "mwms",
                PcDomain = "www.mwms4.com",
                MobileDomain = "m.mwms4.com"
            });
            RequestUrl requestUrl =u.Build(url, context.Request.Headers["User-Agent"]);
            if(requestUrl.IsMobileBrowser && !requestUrl.IsMobileUrl)
            {
                //跳转至手机地址
            } */
            // context.Response.WriteAsync("");
            //  context.Response.Clear
            // SystemData.HttpContext.Response.WriteAsync

        }
    }
}
